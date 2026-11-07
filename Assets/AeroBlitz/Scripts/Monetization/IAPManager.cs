using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if AEROBLITZ_IAP
using UnityEngine.Purchasing;
#endif

namespace AeroBlitz
{
    /// <summary>
    /// Compras con dinero real (Google Play / App Store) usando Unity IAP 5.
    /// Todas las compras se entregan en ProductCatalog.Grant().
    /// Flujo IAP 5: Connect → FetchProducts → FetchPurchases (restaura) → compra → pedido pendiente → entregar → ConfirmPurchase.
    /// </summary>
    public class IAPManager : MonoBehaviour
    {
        public static IAPManager Instance { get; private set; }

        Action<bool, string> _pendingCallback;
        public bool IsReady { get; private set; }

        static bool SimulationAllowed => Application.isEditor || Debug.isDebugBuild;
        static bool RealStorePlatform => Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;

#if AEROBLITZ_IAP
        StoreController _store;
#endif

        void Awake() { Instance = this; }

        /// <summary>Se llama después de inicializar Unity Gaming Services.</summary>
        public async void Initialize()
        {
#if AEROBLITZ_IAP
            if (!RealStorePlatform && !Application.isEditor)
            {
                Debug.Log("[IAP] Plataforma sin tienda: compras simuladas en builds de desarrollo.");
                return;
            }
            try
            {
                _store = UnityIAPServices.StoreController();
                _store.OnStoreConnected += OnStoreConnected;
                _store.OnStoreDisconnected += d => { IsReady = false; Debug.LogWarning("[IAP] Tienda desconectada: " + d.Message); };
                _store.OnProductsFetched += OnProductsFetched;
                _store.OnProductsFetchFailed += f => Debug.LogWarning($"[IAP] No se obtuvieron {f.FailedFetchProducts.Count} productos: {f.FailureReason}");
                _store.OnPurchasesFetched += OnPurchasesFetched;
                _store.OnPurchasesFetchFailed += f => Debug.LogWarning("[IAP] No se pudieron recuperar compras: " + f.Message);
                _store.OnPurchasePending += OnPurchasePending;
                _store.OnPurchaseConfirmed += OnPurchaseConfirmed;
                _store.OnPurchaseFailed += OnPurchaseFailed;
                _store.OnPurchaseDeferred += order => Debug.Log("[IAP] Compra diferida (pendiente de aprobación).");
                _store.OnCheckEntitlement += OnCheckEntitlement;
                await _store.Connect();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[IAP] No se pudo conectar con la tienda: " + e.Message);
                _store = null;
                IsReady = false;
            }
#else
            Debug.Log("[IAP] Paquete com.unity.purchasing no instalado: compras simuladas en Editor.");
            await System.Threading.Tasks.Task.CompletedTask;
#endif
        }

        public string GetPrice(string productId)
        {
#if AEROBLITZ_IAP
            if (_store != null && IsReady && RealStorePlatform)
            {
                var p = _store.GetProductById(productId);
                if (p != null && p.metadata != null && !string.IsNullOrEmpty(p.metadata.localizedPriceString))
                    return p.metadata.localizedPriceString;
            }
#endif
            var def = ProductCatalog.Get(productId);
            return def != null ? def.fallbackPrice : "";
        }

        public void Buy(string productId, Action<bool, string> onDone)
        {
#if AEROBLITZ_IAP
            if (_store != null && IsReady)
            {
                var product = _store.GetProductById(productId);
                if (product != null && product.availableToPurchase)
                {
                    _pendingCallback = onDone;
                    _store.PurchaseProduct(product);
                    return;
                }
                if (!SimulationAllowed) { onDone?.Invoke(false, L.T("iap.not_available")); return; }
            }
#endif
            if (SimulationAllowed)
            {
                Debug.Log("[IAP] Compra SIMULADA: " + productId);
                Deliver(productId);
                onDone?.Invoke(true, L.T("iap.simulated"));
                UIManager.Instance.ShowProtectPurchasesPrompt();
                return;
            }
            onDone?.Invoke(false, L.T("iap.store_down"));
        }

        /// <summary>Requerido por Apple: botón "Restaurar compras".</summary>
        public void RestorePurchases(Action<bool, string> onDone)
        {
#if AEROBLITZ_IAP
            if (_store != null && IsReady && Application.platform == RuntimePlatform.IPhonePlayer)
            {
                _store.RestoreTransactions((ok, msg) => onDone?.Invoke(ok, ok ? L.T("iap.restored") : msg));
                return;
            }
            if (_store != null && IsReady) _store.FetchPurchases(); // Android: vuelve a sincronizar
#endif
            onDone?.Invoke(true, L.T("iap.android_restore"));
        }

        void Deliver(string productId)
        {
            var gm = GameManager.Instance;
            ProductCatalog.Grant(gm.Data, productId);
            gm.Save();
            gm.NotifyChanged();
            if (productId == ProductIds.RemoveAds || productId == ProductIds.VipMonthly) AdsManager.Instance.HideBanner();
        }

#if AEROBLITZ_IAP
        void OnStoreConnected()
        {
            var defs = new List<UnityEngine.Purchasing.ProductDefinition>();
            foreach (var p in ProductCatalog.All)
            {
                var type = p.kind == ProductKind.Consumable ? ProductType.Consumable
                         : p.kind == ProductKind.NonConsumable ? ProductType.NonConsumable
                         : ProductType.Subscription;
                defs.Add(new UnityEngine.Purchasing.ProductDefinition(p.id, type));
            }
            _store.FetchProducts(defs);
        }

        void OnProductsFetched(List<Product> products)
        {
            IsReady = true;
            Debug.Log($"[IAP] Tienda lista: {products.Count} productos.");
            _store.FetchPurchases(); // restaura no consumibles y procesa compras pendientes
            var vip = _store.GetProductById(ProductIds.VipMonthly);
            if (vip != null) _store.CheckEntitlement(vip);
            GameManager.Instance.NotifyChanged(); // actualiza precios localizados en la UI
        }

        /// <summary>Restaura compras no consumibles ya hechas (reinstalación, otro dispositivo).</summary>
        void OnPurchasesFetched(Orders orders)
        {
            bool changed = false;
            foreach (var order in orders.ConfirmedOrders)
                foreach (var item in order.CartOrdered.Items())
                {
                    string id = item.Product?.definition?.id;
                    var def = id != null ? ProductCatalog.Get(id) : null;
                    if (def == null || def.kind == ProductKind.Consumable) continue;
                    if (def.kind == ProductKind.Subscription) continue; // se valida con CheckEntitlement
                    ProductCatalog.Grant(GameManager.Instance.Data, id);
                    changed = true;
                }
            if (changed) { GameManager.Instance.Save(); GameManager.Instance.NotifyChanged(); }
        }

        void OnPurchasePending(PendingOrder order)
        {
            // IMPORTANTE para producción: valida el recibo (tu servidor o validación local de Google/Apple)
            // antes de entregar. Ver LEEME.md.
            var data = GameManager.Instance.Data;
            string txId = order.Info != null ? order.Info.TransactionID : null;
            bool alreadyDelivered = !string.IsNullOrEmpty(txId) && data.deliveredTransactions.Contains(txId);

            // Si la confirmación anterior falló, la tienda vuelve a enviar este pedido: no se entrega otra vez.
            if (!alreadyDelivered)
            {
                foreach (var item in order.CartOrdered.Items())
                {
                    string id = item.Product?.definition?.id;
                    if (!string.IsNullOrEmpty(id)) Deliver(id);
                }
                if (!string.IsNullOrEmpty(txId))
                {
                    data.deliveredTransactions.Add(txId);
                    if (data.deliveredTransactions.Count > 200) data.deliveredTransactions.RemoveAt(0);
                    GameManager.Instance.Save(); // se guarda ANTES de confirmar
                }
            }
            else Debug.Log("[IAP] Pedido reenviado por la tienda; ya estaba entregado. Solo se confirma.");

            _store.ConfirmPurchase(order);

            if (alreadyDelivered) return;
            var cb = _pendingCallback; _pendingCallback = null;
            cb?.Invoke(true, L.T("iap.done"));
            // Sin cuenta, una reinstalación perdería las monedas compradas: se invita a respaldar.
            UIManager.Instance.ShowProtectPurchasesPrompt();
        }

        int _confirmRetries;

        /// <summary>La tienda avisa del resultado de la confirmación: puede ser exitosa o fallida.</summary>
        void OnPurchaseConfirmed(Order order)
        {
            if (order is FailedOrder failed)
            {
                Debug.LogWarning($"[IAP] La confirmación con la tienda FALLÓ: {failed.FailureReason} · {failed.Details}");
                // Reintenta unas veces: al volver a pedir las compras, la tienda reenvía el pedido pendiente.
                if (_confirmRetries < 4) { _confirmRetries++; StartCoroutine(RetryConfirm()); }
                return;
            }
            _confirmRetries = 0;
            Debug.Log("[IAP] Compra confirmada con la tienda.");
        }

        IEnumerator RetryConfirm()
        {
            yield return new WaitForSecondsRealtime(6f * _confirmRetries);
            if (_store != null && IsReady) _store.FetchPurchases();
        }

        void OnPurchaseFailed(FailedOrder order)
        {
            var cb = _pendingCallback; _pendingCallback = null;
            cb?.Invoke(false, order.FailureReason == PurchaseFailureReason.UserCancelled
                ? L.T("iap.cancelled")
                : L.T("iap.failed_fmt", order.FailureReason));
        }

        /// <summary>Estado de la suscripción VIP según la tienda.</summary>
        void OnCheckEntitlement(Entitlement entitlement)
        {
            if (entitlement.Product?.definition?.id != ProductIds.VipMonthly) return;
            bool active = entitlement.Status == EntitlementStatus.FullyEntitled
                       || entitlement.Status == EntitlementStatus.EntitledButNotFinished;
            var d = GameManager.Instance.Data;
            if (d.vip == active) return;
            d.vip = active;
            GameManager.Instance.Save();
            GameManager.Instance.NotifyChanged();
        }
#endif
    }
}
