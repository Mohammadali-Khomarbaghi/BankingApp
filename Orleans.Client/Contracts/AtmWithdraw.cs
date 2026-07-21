using System.Runtime.Serialization;

namespace Orleans.Client.Contracts;

[DataContract]
public record AtmWithdraw
{
    [DataMember]
    public Guid CheckingAccountId { get; init; }
    [DataMember]
    public decimal Amount { get; init; }
}
