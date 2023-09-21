namespace BestStoreApi.Services;

public class OrderHelper
{
    public static decimal ShippingFee { get; } = 5;
    public static Dictionary<string, string> PaymentMethods { get; } = new()
    {
        { "Cash", "Cash on Delivery" },
        { "PayPal", "PayPal"},
        { "Credit Card", "Credit Card"}
    };
    public static List<string> PaymentStatuses { get; } = new()
    {
        "Pending", "Accepted", "Canceled"
    };
    public static List<string> OrderStatues { get; } = new()
    {
        "Created", "Accepted", "Cancelled", "Shipped", "Delivered", "Returned"
    };
    // Recieves a string of product identifiers, seperated by '-'
    // Example 9-9-7-9-6
    //
    // Return a list of pairs (dictionary)
    //  -the pair name is the product Id
    //  -the pair valie is the product quantity
    // Example:
    //{
    //  9: 3,
    //  7: 1,
    //  6:1
    //}
    public static Dictionary<int, int>  GetProductDictionary(string productIdentifiers)
    {
        var productDictionary = new Dictionary<int, int>();
        if (productIdentifiers.Length > 0)
        {
            string[] productIdArray = productIdentifiers.Split('-');
            foreach (string productId in productIdArray)
            {
                try
                {
                    int id = int.Parse(productId);
                    if (productDictionary.ContainsKey(id))
                    {
                        productDictionary[id] += 1;
                    }
                    else
                    {
                        productDictionary.Add(id, 1);
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        return productDictionary;
    }
}
