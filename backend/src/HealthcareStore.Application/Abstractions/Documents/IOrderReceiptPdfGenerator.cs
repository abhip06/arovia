using System.IO;
using HealthcareStore.Application.Orders.Receipts;

namespace HealthcareStore.Application.Abstractions.Documents;

public interface IOrderReceiptPdfGenerator
{
    void Generate(
        OrderReceiptModel receipt,
        Stream destination);
}