
public record TestDataPerson
{
    public TestDataPerson(string nameFirst, string nameLast, string street, string city, string postalCode)
    {
        NameFirst = nameFirst;
        NameLast = nameLast;
        Street = street;
        City = city;
        PostalCode = postalCode;
    }
    public string NameFirst { get; set; } = "";
    public string NameLast { get; set; } = "";
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string PostalCode { get; set; } = "";

    public override string ToString()
    {
        return $"{NameFirst} {NameLast} {Street} {City} {PostalCode}";
    }
};