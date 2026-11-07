using System.Collections.Generic;

namespace AeroBlitz
{
    public class Country
    {
        public readonly string code;
        readonly string[] _names; // Español, English, Français
        public Country(string code, string es, string en, string fr) { this.code = code; _names = new[] { es, en, fr }; }
        public string Name => _names[(int)L.Current];
    }

    public static class Countries
    {
        static readonly List<Country> _all = new List<Country>
        {
            new Country("AR","Argentina","Argentina","Argentine"),
            new Country("BO","Bolivia","Bolivia","Bolivie"),
            new Country("BR","Brasil","Brazil","Brésil"),
            new Country("CL","Chile","Chile","Chili"),
            new Country("CO","Colombia","Colombia","Colombie"),
            new Country("CR","Costa Rica","Costa Rica","Costa Rica"),
            new Country("CU","Cuba","Cuba","Cuba"),
            new Country("DO","República Dominicana","Dominican Republic","République dominicaine"),
            new Country("EC","Ecuador","Ecuador","Équateur"),
            new Country("SV","El Salvador","El Salvador","Salvador"),
            new Country("GT","Guatemala","Guatemala","Guatemala"),
            new Country("HN","Honduras","Honduras","Honduras"),
            new Country("MX","México","Mexico","Mexique"),
            new Country("NI","Nicaragua","Nicaragua","Nicaragua"),
            new Country("PA","Panamá","Panama","Panama"),
            new Country("PY","Paraguay","Paraguay","Paraguay"),
            new Country("PE","Perú","Peru","Pérou"),
            new Country("PR","Puerto Rico","Puerto Rico","Porto Rico"),
            new Country("UY","Uruguay","Uruguay","Uruguay"),
            new Country("VE","Venezuela","Venezuela","Venezuela"),
            new Country("ES","España","Spain","Espagne"),
            new Country("US","Estados Unidos","United States","États-Unis"),
            new Country("CA","Canadá","Canada","Canada"),
            new Country("GQ","Guinea Ecuatorial","Equatorial Guinea","Guinée équatoriale"),
            new Country("DE","Alemania","Germany","Allemagne"),
            new Country("AU","Australia","Australia","Australie"),
            new Country("AT","Austria","Austria","Autriche"),
            new Country("BE","Bélgica","Belgium","Belgique"),
            new Country("CN","China","China","Chine"),
            new Country("KR","Corea del Sur","South Korea","Corée du Sud"),
            new Country("DK","Dinamarca","Denmark","Danemark"),
            new Country("EG","Egipto","Egypt","Égypte"),
            new Country("AE","Emiratos Árabes Unidos","United Arab Emirates","Émirats arabes unis"),
            new Country("PH","Filipinas","Philippines","Philippines"),
            new Country("FI","Finlandia","Finland","Finlande"),
            new Country("FR","Francia","France","France"),
            new Country("GR","Grecia","Greece","Grèce"),
            new Country("HT","Haití","Haiti","Haïti"),
            new Country("IN","India","India","Inde"),
            new Country("ID","Indonesia","Indonesia","Indonésie"),
            new Country("IE","Irlanda","Ireland","Irlande"),
            new Country("IL","Israel","Israel","Israël"),
            new Country("IT","Italia","Italy","Italie"),
            new Country("JM","Jamaica","Jamaica","Jamaïque"),
            new Country("JP","Japón","Japan","Japon"),
            new Country("MA","Marruecos","Morocco","Maroc"),
            new Country("NG","Nigeria","Nigeria","Nigéria"),
            new Country("NO","Noruega","Norway","Norvège"),
            new Country("NZ","Nueva Zelanda","New Zealand","Nouvelle-Zélande"),
            new Country("NL","Países Bajos","Netherlands","Pays-Bas"),
            new Country("PK","Pakistán","Pakistan","Pakistan"),
            new Country("PL","Polonia","Poland","Pologne"),
            new Country("PT","Portugal","Portugal","Portugal"),
            new Country("GB","Reino Unido","United Kingdom","Royaume-Uni"),
            new Country("CZ","República Checa","Czech Republic","République tchèque"),
            new Country("RO","Rumania","Romania","Roumanie"),
            new Country("RU","Rusia","Russia","Russie"),
            new Country("SA","Arabia Saudita","Saudi Arabia","Arabie saoudite"),
            new Country("ZA","Sudáfrica","South Africa","Afrique du Sud"),
            new Country("SE","Suecia","Sweden","Suède"),
            new Country("CH","Suiza","Switzerland","Suisse"),
            new Country("TH","Tailandia","Thailand","Thaïlande"),
            new Country("TW","Taiwán","Taiwan","Taïwan"),
            new Country("TT","Trinidad y Tobago","Trinidad and Tobago","Trinité-et-Tobago"),
            new Country("TR","Turquía","Turkey","Turquie"),
            new Country("UA","Ucrania","Ukraine","Ukraine"),
            new Country("VN","Vietnam","Vietnam","Viêt Nam"),
            new Country("DZ","Argelia","Algeria","Algérie"),
            new Country("BD","Bangladés","Bangladesh","Bangladesh"),
            new Country("KE","Kenia","Kenya","Kenya"),
            new Country("MY","Malasia","Malaysia","Malaisie"),
            new Country("SG","Singapur","Singapore","Singapour"),
            new Country("HU","Hungría","Hungary","Hongrie"),
            new Country("SN","Senegal","Senegal","Sénégal"),
            new Country("CI","Costa de Marfil","Ivory Coast","Côte d'Ivoire"),
            new Country("CM","Camerún","Cameroon","Cameroun"),
            new Country("TN","Túnez","Tunisia","Tunisie"),
            new Country("LU","Luxemburgo","Luxembourg","Luxembourg"),
            new Country("OT","Otro país","Other country","Autre pays"),
        };

        /// <summary>Lista ordenada alfabéticamente en el idioma actual ("Otro país" al final).</summary>
        public static List<Country> All
        {
            get
            {
                var list = new List<Country>(_all);
                var culture = L.Culture;
                list.Sort((a, b) =>
                {
                    if (a.code == b.code) return 0;
                    if (a.code == "OT") return 1;
                    if (b.code == "OT") return -1;
                    return string.Compare(a.Name, b.Name, culture, System.Globalization.CompareOptions.IgnoreCase);
                });
                return list;
            }
        }

        public static string NameOf(string code)
        {
            if (code == null) return "?";
            var c = _all.Find(x => x.code == code);
            return c != null ? c.Name : code;
        }
    }
}
