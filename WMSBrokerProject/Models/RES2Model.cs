using System.Xml;
using System.Xml.Serialization;

namespace WMSBrokerProject.Models
{
    public class RES2Model
    {
        public RES2Model()
        {
            //this.Envelope = new Envelope();
        }
        
        public Dictionary<string, object> WMSBeheerderAttributes { get; set; }
        public int? AantalAansluitingenCount { get; set; }
        //public string CityName { get; set; }
        //public string StreetName { get; set; }
        //public string HouseNumber { get; set; }
        //public string ZipCode { get; set; }
    }
    //public class RES2Root
    //{
    //    public class Header
    //    {
    //        public string MessageID { get; set; }
    //        public string CorrelationID { get; set; }
    //    }
    //    public class Aanvraag
    //    {
    //        public Aanvraag()
    //        {
    //            Aanvrager = new Aanvrager();
    //        }
    //        public string Aanleveringswijze { get; set; }
    //        public string AantalBijlagen { get; set; }
    //        public string AanvraagDatum { get; set; }
    //        public Aanvrager Aanvrager { get; set; }
    //    }
    //    public class Aanvrager
    //    {
    //        public string AanvragerID { get; set; }
    //        public string IsProfessioneel { get; set; }
    //    }
    //    public class RelatieGegevens
    //    {

    //    }
    //}

    // using System.Xml.Serialization;
    // XmlSerializer serializer = new XmlSerializer(typeof(Envelope));
    // using (StringReader reader = new StringReader(xml))
    // {
    //    var test = (Envelope)serializer.Deserialize(reader);
    // }

    [XmlRoot(ElementName = "Header")]
    public class Header
    {

        [XmlElement(ElementName = "MessageID")]
        public string MessageID { get; set; }

        [XmlElement(ElementName = "CorrelationID")]
        public string CorrelationID { get; set; }

        [XmlElement(ElementName = "RepeatCount")]
        public string RepeatCount { get; set; }

        [XmlElement(ElementName = "SenderID")]
        public string SenderID { get; set; }

        [XmlElement(ElementName = "RecipientID")]
        public string RecipientID { get; set; }

        [XmlElement(ElementName = "CreateTime")]
        public string CreateTime { get; set; }

        [XmlElement(ElementName = "SendTime")]
        public string SendTime { get; set; }

        [XmlElement(ElementName = "MessageVersion")]
        public string MessageVersion { get; set; }
    }

    [XmlRoot(ElementName = "ContactPersoon")]
    public class ContactPersoon
    {

        [XmlElement(ElementName = "Aanhef")]
        public string Aanhef { get; set; }

        [XmlElement(ElementName = "Achternaam")]
        public string Achternaam { get; set; }

        [XmlElement(ElementName = "Tussenvoegsels")]
        public string Tussenvoegsels { get; set; }

        [XmlElement(ElementName = "Voorletters")]
        public string Voorletters { get; set; }

        [XmlElement(ElementName = "Naam")]
        public string Naam { get; set; }

        [XmlElement(ElementName = "Emailadres")]
        public string Emailadres { get; set; }

        [XmlElement(ElementName = "TelefoonnummerMobiel")]
        public string TelefoonnummerMobiel { get; set; }

        [XmlElement(ElementName = "NotificatieVoorkeur")]
        public string NotificatieVoorkeur { get; set; }

        [XmlElement(ElementName = "ContactKenmerk")]
        public string ContactKenmerk { get; set; }
    }

    [XmlRoot(ElementName = "Bedrijfsgegevens")]
    public class Bedrijfsgegevens
    {
        public Bedrijfsgegevens()
        {
            this.ContactPersoon = new ContactPersoon();
        }

        [XmlElement(ElementName = "Bedrijfsnaam")]
        public string Bedrijfsnaam { get; set; }

        [XmlElement(ElementName = "ContactPersoon")]
        public ContactPersoon ContactPersoon { get; set; }

        [XmlElement(ElementName = "BTWNummer")]
        public string BTWNummer { get; set; }

        [XmlElement(ElementName = "IsBTWVerlegd")]
        public string IsBTWVerlegd { get; set; }

        [XmlElement(ElementName = "KvKNummer")]
        public string KvKNummer { get; set; }
    }

    [XmlRoot(ElementName = "Persoonsgegevens")]
    public class Persoonsgegevens
    {

        [XmlElement(ElementName = "Aanhef")]
        public string Aanhef { get; set; }

        [XmlElement(ElementName = "Achternaam")]
        public string Achternaam { get; set; }

        [XmlElement(ElementName = "Tussenvoegsels")]
        public string Tussenvoegsels { get; set; }

        [XmlElement(ElementName = "Voorletters")]
        public string Voorletters { get; set; }
    }

    [XmlRoot(ElementName = "Adres")]
    public class Adres
    {

        [XmlElement(ElementName = "Huisnummer")]
        public string Huisnummer { get; set; }

        [XmlElement(ElementName = "HuisnummerToevoeging")]
        public string HuisnummerToevoeging { get; set; }

        [XmlElement(ElementName = "Land")]
        public string Land { get; set; }

        [XmlElement(ElementName = "Plaats")]
        public string Plaats { get; set; }

        [XmlElement(ElementName = "Postcode")]
        public string Postcode { get; set; }

        [XmlElement(ElementName = "Straat")]
        public string Straat { get; set; }

        [XmlElement(ElementName = "StraatNEN")]
        public string StraatNEN { get; set; }

        [XmlElement(ElementName = "PostbusNummer")]
        public string PostbusNummer { get; set; }

        [XmlElement(ElementName = "PostbusPostcode")]
        public string PostbusPostcode { get; set; }

        [XmlElement(ElementName = "PostbusPlaats")]
        public string PostbusPlaats { get; set; }
    }

    [XmlRoot(ElementName = "RelatieGegevens")]
    public class RelatieGegevens
    {
        public RelatieGegevens()
        {
            this.Bedrijfsgegevens = new Bedrijfsgegevens();
            this.Persoonsgegevens = new Persoonsgegevens();
            this.Adres = new Adres();
        }

        [XmlElement(ElementName = "IsBedrijf")]
        public string IsBedrijf { get; set; }

        [XmlElement(ElementName = "Bedrijfsgegevens")]
        public Bedrijfsgegevens Bedrijfsgegevens { get; set; }

        [XmlElement(ElementName = "Persoonsgegevens")]
        public Persoonsgegevens Persoonsgegevens { get; set; }

        [XmlElement(ElementName = "Adres")]
        public Adres Adres { get; set; }

        [XmlElement(ElementName = "Emailadres")]
        public string Emailadres { get; set; }

        [XmlElement(ElementName = "Faxnummer")]
        public string Faxnummer { get; set; }

        [XmlElement(ElementName = "Telefoonnummer")]
        public string Telefoonnummer { get; set; }

        [XmlElement(ElementName = "TelefoonnummerMobiel")]
        public string TelefoonnummerMobiel { get; set; }
    }

    [XmlRoot(ElementName = "Aanvrager")]
    public class Aanvrager
    {
        public Aanvrager()
        {
            this.RelatieGegevens = new RelatieGegevens();
        }

        [XmlElement(ElementName = "AanvragerID")]
        public string AanvragerID { get; set; }

        [XmlElement(ElementName = "IsProfessioneel")]
        public string IsProfessioneel { get; set; }

        [XmlElement(ElementName = "NotificatieVoorkeur")]
        public string NotificatieVoorkeur { get; set; }

        [XmlElement(ElementName = "NotificatieNiveau")]
        public string NotificatieNiveau { get; set; }

        [XmlElement(ElementName = "RelatieGegevens")]
        public RelatieGegevens RelatieGegevens { get; set; }
    }

    [XmlRoot(ElementName = "Bijlage")]
    public class Bijlage
    {

        [XmlElement(ElementName = "VraagCode")]
        public string VraagCode { get; set; }

        [XmlElement(ElementName = "NetbeheerderCode")]
        public string NetbeheerderCode { get; set; }

        [XmlElement(ElementName = "Bestandsgrootte")]
        public string Bestandsgrootte { get; set; }

        [XmlElement(ElementName = "Bestandsnaam")]
        public string Bestandsnaam { get; set; }

        [XmlElement(ElementName = "BijlageID")]
        public string BijlageID { get; set; }

        [XmlElement(ElementName = "MimeType")]
        public string MimeType { get; set; }

        [XmlElement(ElementName = "Omschrijving")]
        public string Omschrijving { get; set; }
    }

    [XmlRoot(ElementName = "Bijlagen")]
    public class Bijlagen
    {
        public Bijlagen()
        {
            this.Bijlage = new Bijlage();
        }

        [XmlElement(ElementName = "Omschrijving")]
        public string Omschrijving { get; set; }

        [XmlElement(ElementName = "Bijlage")]
        public Bijlage Bijlage { get; set; }
    }

    [XmlRoot(ElementName = "Coordinerende")]
    public class Coordinerende
    {
        public Coordinerende()
        {
            this.RelatieGegevens = new RelatieGegevens();
        }

        [XmlElement(ElementName = "HeeftMeerdereCoordinerenden")]
        public string HeeftMeerdereCoordinerenden { get; set; }

        [XmlElement(ElementName = "OrganisatieCode")]
        public string OrganisatieCode { get; set; }

        [XmlElement(ElementName = "VerzorgingsgebiedContractCode")]
        public string VerzorgingsgebiedContractCode { get; set; }

        [XmlElement(ElementName = "RelatieGegevens")]
        public RelatieGegevens RelatieGegevens { get; set; }
    }



    [XmlRoot(ElementName = "CorrespondentieAdres")]
    public class CorrespondentieAdres
    {
        public CorrespondentieAdres()
        {
            this.RelatieGegevens = new RelatieGegevens();
        }

        [XmlElement(ElementName = "isAanvrager")]
        public string IsAanvrager { get; set; }

        [XmlElement(ElementName = "isAfwijkendAdres")]
        public string IsAfwijkendAdres { get; set; }

        [XmlElement(ElementName = "relatieGegevens")]
        public RelatieGegevens RelatieGegevens { get; set; }
    }

    [XmlRoot(ElementName = "FactuurOntvanger")]
    public class FactuurOntvanger
    {
        public FactuurOntvanger()
        {
            this.RelatieGegevens = new RelatieGegevens();
        }

        [XmlElement(ElementName = "IsAanvrager")]
        public string IsAanvrager { get; set; }

        [XmlElement(ElementName = "IsAfwijkendAdres")]
        public string IsAfwijkendAdres { get; set; }

        [XmlElement(ElementName = "RelatieGegevens")]
        public RelatieGegevens RelatieGegevens { get; set; }
    }

    [XmlRoot(ElementName = "Coordinaten")]
    public class Coordinaten
    {

        [XmlElement(ElementName = "Lengtegraad")]
        public string Lengtegraad { get; set; }

        [XmlElement(ElementName = "Breedtegraad")]
        public string Breedtegraad { get; set; }

        [XmlElement(ElementName = "Codestelsel")]
        public string Codestelsel { get; set; }
    }

    [XmlRoot(ElementName = "AdditioneleVraag")]
    public class AdditioneleVraag
    {

        [XmlElement(ElementName = "VraagCode")]
        public string VraagCode { get; set; }

        [XmlElement(ElementName = "NetbeheerderCode")]
        public string NetbeheerderCode { get; set; }

        [XmlElement(ElementName = "Vraag")]
        public string Vraag { get; set; }

        [XmlElement(ElementName = "VraagType")]
        public string VraagType { get; set; }

        [XmlElement(ElementName = "AntwoordCode")]
        public string AntwoordCode { get; set; }

        [XmlElement(ElementName = "Antwoord")]
        public string Antwoord { get; set; }
    }

    [XmlRoot(ElementName = "AdditioneleVragen")]
    public class AdditioneleVragen
    {
        public AdditioneleVragen()
        {
            this.AdditioneleVraag = new AdditioneleVraag();
        }

        [XmlElement(ElementName = "AdditioneleVraag")]
        public AdditioneleVraag AdditioneleVraag { get; set; }
    }

    [XmlRoot(ElementName = "Product")]
    public class Product
    {
        public Product()
        {
            this.AdditioneleVragen = new AdditioneleVragen();
        }

        [XmlElement(ElementName = "DisciplineCode")]
        public string DisciplineCode { get; set; }

        [XmlElement(ElementName = "DisciplineID")]
        public string DisciplineID { get; set; }

        [XmlElement(ElementName = "NetbeheerderCode")]
        public string NetbeheerderCode { get; set; }

        [XmlElement(ElementName = "AdditioneleVragen")]
        public AdditioneleVragen AdditioneleVragen { get; set; }

        [XmlElement(ElementName = "EANCode")]
        public string EANCode { get; set; }

        [XmlElement(ElementName = "IsEANHandmatig")]
        public string IsEANHandmatig { get; set; }

        [XmlElement(ElementName = "IsEANKeuze")]
        public string IsEANKeuze { get; set; }

        [XmlElement(ElementName = "Specificatie")]
        public string Specificatie { get; set; }

        [XmlElement(ElementName = "DienstCode")]
        public string DienstCode { get; set; }

        [XmlElement(ElementName = "SubdienstCode")]
        public string SubdienstCode { get; set; }

        [XmlElement(ElementName = "ProductCode")]
        public string ProductCode { get; set; }

        [XmlElement(ElementName = "TariefCode")]
        public string TariefCode { get; set; }

        [XmlElement(ElementName = "BedrijfseigenKenmerk")]
        public string BedrijfseigenKenmerk { get; set; }

        [XmlElement(ElementName = "PrijsIndicatieEenmalig")]
        public string PrijsIndicatieEenmalig { get; set; }

        [XmlElement(ElementName = "PrijsIndicatieMaandelijks")]
        public string PrijsIndicatieMaandelijks { get; set; }

        [XmlElement(ElementName = "HeeftOverlap")]
        public string HeeftOverlap { get; set; }

        [XmlElement(ElementName = "ProcesStatus")]
        public string ProcesStatus { get; set; }

        [XmlElement(ElementName = "ProcesVariant")]
        public string ProcesVariant { get; set; }
    }

    [XmlRoot(ElementName = "Produkten")]
    public class Produkten
    {
        public Produkten()
        {
            this.Product = new Product();
        }

        [XmlElement(ElementName = "Product")]
        public Product Product { get; set; }
    }

    [XmlRoot(ElementName = "ToekomstigeGebruiker")]
    public class ToekomstigeGebruiker
    {
        public ToekomstigeGebruiker()
        {
            this.RelatieGegevens = new RelatieGegevens();
        }
        [XmlElement(ElementName = "RelatieGegevens")]
        public RelatieGegevens RelatieGegevens { get; set; }

        [XmlElement(ElementName = "SoortCode")]
        public string SoortCode { get; set; }
    }

    [XmlRoot(ElementName = "AansluitObject")]
    public class AansluitObject
    {
        public AansluitObject()
        {
            this.ToekomstigeGebruiker = new ToekomstigeGebruiker();
            this.Produkten = new Produkten();
            this.Adres = new Adres();
            this.Coordinaten = new Coordinaten();
        }
        [XmlElement(ElementName = "Adres")]
        public Adres Adres { get; set; }

        [XmlElement(ElementName = "BagObjectCode")]
        public string BagObjectCode { get; set; }

        [XmlElement(ElementName = "Coordinaten")]
        public Coordinaten Coordinaten { get; set; }

        [XmlElement(ElementName = "HeeftMeerdereBAGIds")]
        public string HeeftMeerdereBAGIds { get; set; }

        [XmlElement(ElementName = "IsPostcodeBijBenadering")]
        public string IsPostcodeBijBenadering { get; set; }

        [XmlElement(ElementName = "LocatieOmschrijving")]
        public string LocatieOmschrijving { get; set; }

        [XmlElement(ElementName = "ObjectID")]
        public string ObjectID { get; set; }

        [XmlElement(ElementName = "ObjectType")]
        public string ObjectType { get; set; }

        [XmlElement(ElementName = "Produkten")]
        public Produkten Produkten { get; set; }

        [XmlElement(ElementName = "ToekomstigeGebruiker")]
        public ToekomstigeGebruiker ToekomstigeGebruiker { get; set; }

        [XmlElement(ElementName = "WensJaar")]
        public string WensJaar { get; set; }

        [XmlElement(ElementName = "WensWeek")]
        public string WensWeek { get; set; }
    }

    [XmlRoot(ElementName = "AansluitObjecten")]
    public class AansluitObjecten
    {
        public AansluitObjecten()
        {
            this.AansluitObject = new AansluitObject();
        }

        [XmlElement(ElementName = "AansluitObject")]
        public AansluitObject AansluitObject { get; set; }
    }

    [XmlRoot(ElementName = "GeaccepteerdeVoorwaarde")]
    public class GeaccepteerdeVoorwaarde
    {

        [XmlElement(ElementName = "TitelVoorwaarde")]
        public string TitelVoorwaarde { get; set; }

        [XmlElement(ElementName = "URLVoorwaarde")]
        public string URLVoorwaarde { get; set; }
    }

    [XmlRoot(ElementName = "GeaccepteerdeVoorwaarden")]
    public class GeaccepteerdeVoorwaarden
    {
        public GeaccepteerdeVoorwaarden()
        {
            this.GeaccepteerdeVoorwaarde = new GeaccepteerdeVoorwaarde();
        }

        [XmlElement(ElementName = "GeaccepteerdeVoorwaarde")]
        public GeaccepteerdeVoorwaarde GeaccepteerdeVoorwaarde { get; set; }
    }

    [XmlRoot(ElementName = "BTWComponent")]
    public class BTWComponent
    {

        [XmlElement(ElementName = "BTWPercentage")]
        public string BTWPercentage { get; set; }

        [XmlElement(ElementName = "BTWBedrag")]
        public string BTWBedrag { get; set; }
    }

    [XmlRoot(ElementName = "BTWPerTariefSoort")]
    public class BTWPerTariefSoort
    {
        public BTWPerTariefSoort()
        {
            this.BTWComponent = new BTWComponent();
        }
        [XmlElement(ElementName = "BTWComponent")]
        public BTWComponent BTWComponent { get; set; }
    }

    [XmlRoot(ElementName = "PrijscomponentRegel")]
    public class PrijscomponentRegel
    {

        [XmlElement(ElementName = "RegelNummer")]
        public string RegelNummer { get; set; }

        [XmlElement(ElementName = "TariefCode")]
        public string TariefCode { get; set; }

        [XmlElement(ElementName = "BedrijfseigenKenmerk")]
        public string BedrijfseigenKenmerk { get; set; }

        [XmlElement(ElementName = "Omschrijving")]
        public string Omschrijving { get; set; }

        [XmlElement(ElementName = "IndicatieBezwaar")]
        public string IndicatieBezwaar { get; set; }

        [XmlElement(ElementName = "PrijscomponentCode")]
        public string PrijscomponentCode { get; set; }

        [XmlElement(ElementName = "Aantal")]
        public string Aantal { get; set; }

        [XmlElement(ElementName = "PrijsPerEenheid")]
        public string PrijsPerEenheid { get; set; }

        [XmlElement(ElementName = "Bedrag")]
        public string Bedrag { get; set; }

        [XmlElement(ElementName = "BtwPercentage")]
        public string BtwPercentage { get; set; }
    }

    [XmlRoot(ElementName = "PrijscomponentRegels")]
    public class PrijscomponentRegels
    {
        public PrijscomponentRegels()
        {
            this.PrijscomponentRegel = new();
        }

        [XmlElement(ElementName = "PrijscomponentRegel")]
        public PrijscomponentRegel PrijscomponentRegel { get; set; }
    }

    [XmlRoot(ElementName = "ProductRegel")]
    public class ProductRegel
    {
        public ProductRegel()
        {
            this.PrijscomponentRegels = new PrijscomponentRegels();
        }

        [XmlElement(ElementName = "RegelNummer")]
        public string RegelNummer { get; set; }

        [XmlElement(ElementName = "TariefCode")]
        public string TariefCode { get; set; }

        [XmlElement(ElementName = "BedrijfseigenKenmerk")]
        public string BedrijfseigenKenmerk { get; set; }

        [XmlElement(ElementName = "Omschrijving")]
        public string Omschrijving { get; set; }

        [XmlElement(ElementName = "IndicatieBezwaar")]
        public string IndicatieBezwaar { get; set; }

        [XmlElement(ElementName = "ObjectID")]
        public string ObjectID { get; set; }

        [XmlElement(ElementName = "DisciplineID")]
        public string DisciplineID { get; set; }

        [XmlElement(ElementName = "ProductCode")]
        public string ProductCode { get; set; }

        [XmlElement(ElementName = "PrijscomponentRegels")]
        public PrijscomponentRegels PrijscomponentRegels { get; set; }
    }

    [XmlRoot(ElementName = "ProductRegels")]
    public class ProductRegels
    {
        public ProductRegels()
        {
            this.ProductRegel = new ProductRegel();
        }
        [XmlElement(ElementName = "ProductRegel")]
        public ProductRegel ProductRegel { get; set; }
    }

    [XmlRoot(ElementName = "VerkoopDocument")]
    public class VerkoopDocument
    {
        public VerkoopDocument()
        {
            this.ProductRegels = new ProductRegels();
            this.BTWPerTariefSoort = new BTWPerTariefSoort();
            this.GeaccepteerdeVoorwaarden = new GeaccepteerdeVoorwaarden();
        }
        [XmlElement(ElementName = "VerkoopDocumentNummer")]
        public string VerkoopDocumentNummer { get; set; }

        [XmlElement(ElementName = "AanvraagID")]
        public string AanvraagID { get; set; }

        [XmlElement(ElementName = "NetbeheerderCode")]
        public string NetbeheerderCode { get; set; }

        [XmlElement(ElementName = "AanmaakDatum")]
        public string AanmaakDatum { get; set; }

        [XmlElement(ElementName = "WijzigingsDatum")]
        public string WijzigingsDatum { get; set; }

        [XmlElement(ElementName = "VervalDatum")]
        public string VervalDatum { get; set; }

        [XmlElement(ElementName = "AcceptatieDatum")]
        public string AcceptatieDatum { get; set; }

        [XmlElement(ElementName = "GeaccepteerdeVoorwaarden")]
        public GeaccepteerdeVoorwaarden GeaccepteerdeVoorwaarden { get; set; }

        [XmlElement(ElementName = "AnnuleringsDatum")]
        public string AnnuleringsDatum { get; set; }

        [XmlElement(ElementName = "WeigeringsDatum")]
        public string WeigeringsDatum { get; set; }

        [XmlElement(ElementName = "ProcesVariant")]
        public string ProcesVariant { get; set; }

        [XmlElement(ElementName = "ProcesStatus")]
        public string ProcesStatus { get; set; }

        [XmlElement(ElementName = "TotaalBedragExBtw")]
        public string TotaalBedragExBtw { get; set; }

        [XmlElement(ElementName = "TotaalBedragIncBtw")]
        public string TotaalBedragIncBtw { get; set; }

        [XmlElement(ElementName = "TotaalBTWBedrag")]
        public string TotaalBTWBedrag { get; set; }

        [XmlElement(ElementName = "BTWPerTariefSoort")]
        public BTWPerTariefSoort BTWPerTariefSoort { get; set; }

        [XmlElement(ElementName = "ProductRegels")]
        public ProductRegels ProductRegels { get; set; }
    }

    [XmlRoot(ElementName = "VerkoopDocumenten")]
    public class VerkoopDocumenten
    {
        public VerkoopDocumenten()
        {
            this.VerkoopDocument = new VerkoopDocument();
        }
        [XmlElement(ElementName = "VerkoopDocument")]
        public VerkoopDocument VerkoopDocument { get; set; }
    }

    [XmlRoot(ElementName = "Aanvraag")]
    public class Aanvraag
    {
        public Aanvraag()
        {
            this.Aanvrager = new Aanvrager();
            this.Bijlagen = new Bijlagen();
            this.ContactPersoon = new ContactPersoon();
            this.CorrespondentieAdres = new CorrespondentieAdres();
            this.Coordinerende = new Coordinerende();
            this.FactuurOntvanger = new FactuurOntvanger();
            this.FactuurOntvanger = new FactuurOntvanger();
            this.AansluitObjecten = new AansluitObjecten();
            this.VerkoopDocumenten = new VerkoopDocumenten();
        }

        [XmlElement(ElementName = "Aanleveringswijze")]
        public string Aanleveringswijze { get; set; }

        [XmlElement(ElementName = "AantalBijlagen")]
        public string AantalBijlagen { get; set; }

        [XmlElement(ElementName = "AanvraagDatum")]
        public string AanvraagDatum { get; set; }

        [XmlElement(ElementName = "AanmaakDatum")]
        public string AanmaakDatum { get; set; }

        [XmlElement(ElementName = "AanvraagVersie")]
        public string AanvraagVersie { get; set; }

        [XmlElement(ElementName = "WijzigingsDatum")]
        public string WijzigingsDatum { get; set; }

        [XmlElement(ElementName = "AanvraagID")]
        public string AanvraagID { get; set; }

        [XmlElement(ElementName = "Aanvrager")]
        public Aanvrager Aanvrager { get; set; }

        [XmlElement(ElementName = "AanvragerReferentie")]
        public string AanvragerReferentie { get; set; }

        [XmlElement(ElementName = "Bijlagen")]
        public Bijlagen Bijlagen { get; set; }

        [XmlElement(ElementName = "ContactPersoon")]
        public ContactPersoon ContactPersoon { get; set; }

        [XmlElement(ElementName = "Coordinerende")]
        public Coordinerende Coordinerende { get; set; }

        [XmlElement(ElementName = "CorrespondentieAdres")]
        public CorrespondentieAdres CorrespondentieAdres { get; set; }

        [XmlElement(ElementName = "FactuurOntvanger")]
        public FactuurOntvanger FactuurOntvanger { get; set; }

        [XmlElement(ElementName = "IsCluster")]
        public string IsCluster { get; set; }

        [XmlElement(ElementName = "IsMix")]
        public string IsMix { get; set; }

        [XmlElement(ElementName = "IsNamensAanvragende")]
        public string IsNamensAanvragende { get; set; }

        [XmlElement(ElementName = "AansluitObjecten")]
        public AansluitObjecten AansluitObjecten { get; set; }

        [XmlElement(ElementName = "VerkoopDocumenten")]
        public VerkoopDocumenten VerkoopDocumenten { get; set; }
    }

    [XmlRoot(ElementName = "Result")]
    public class Result
    {

        [XmlElement(ElementName = "ResultCode")]
        public string ResultCode { get; set; }

        [XmlElement(ElementName = "ResultText")]
        public string ResultText { get; set; }
    }

    [XmlRoot(ElementName = "OphalenAanvraagResponse")]
    public class OphalenAanvraagResponse
    {
        public OphalenAanvraagResponse()
        {
            this.Header = new Header();
            this.Aanvraag = new Aanvraag();
            this.Result = new Result();
        }
        [XmlElement(ElementName = "Header")]
        public Header Header { get; set; }

        [XmlElement(ElementName = "Aanvraag")]
        public Aanvraag Aanvraag { get; set; }

        [XmlElement(ElementName = "Result")]
        public Result Result { get; set; }
    }

    [XmlRoot(ElementName = "Body")]
    public class Body
    {
        public Body()
        {
            this.OphalenAanvraagResponse = new OphalenAanvraagResponse();
        }

        [XmlElement(ElementName = "OphalenAanvraagResponse")]
        public OphalenAanvraagResponse OphalenAanvraagResponse { get; set; }
    }

    [XmlRoot(ElementName = "Envelope")]
    public class Envelope
    {
        public Envelope()
        {
            this.Body = new Body();
        }
        [XmlElement(ElementName = "Header")]
        public object Header { get; set; }

        [XmlElement(ElementName = "Body")]
        public Body Body { get; set; }

        [XmlAttribute(AttributeName = "soapenv")]
        public string Soapenv { get; set; }

        [XmlAttribute(AttributeName = "v01")]
        public string V01 { get; set; }

        [XmlAttribute(AttributeName = "v011")]
        public string V011 { get; set; }

        [XmlText]
        public string Text { get; set; }
    }



}
