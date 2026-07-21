using System.Runtime.Serialization;

namespace Orleans.Client.Contracts;

[DataContract]
public record CreateCheckingAccount
{
    [DataMember]
    public decimal OpeningBalance { get; init; }
}
