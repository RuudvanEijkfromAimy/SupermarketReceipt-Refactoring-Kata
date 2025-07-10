using System.Collections.Generic;
using System.Globalization;

namespace SupermarketReceipt
{
    public class ShoppingCart
    {
        private readonly List<ProductQuantity> _items = new List<ProductQuantity>();
        private readonly Dictionary<Product, double> _productQuantities = new Dictionary<Product, double>();
        private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en-GB");


        public List<ProductQuantity> GetItems()
        {
            return new List<ProductQuantity>(_items);
        }

        public void AddItem(Product product)
        {
            AddItemQuantity(product, 1.0);
        }


        public void AddItemQuantity(Product product, double quantity)
        {
            _items.Add(new ProductQuantity(product, quantity));

            if (_productQuantities.ContainsKey(product))
            {
                // there's already a number of this product type in the cart, add to that number
                _productQuantities[product] += quantity;
            }
            else
            {
                // add a new product type to cart
                _productQuantities.Add(product, quantity);
            }
        }

        public void HandleOffers(Receipt receipt, Dictionary<Product, Offer> offers, SupermarketCatalog catalog)
        {
            // loop through each product type in cart
            foreach (var p in _productQuantities.Keys)
            {
                int quantity = (int) _productQuantities[p];
                if (offers.ContainsKey(p))
                {
                    Offer offer = offers[p];
                    double unitPrice = catalog.GetUnitPrice(p);
                    Discount discount = null;
                    int x = 1;
                    int numberOfXs = quantity;

                    switch (offer.OfferType)
                    {
                        case SpecialOfferType.ThreeForTwo:
                            {
                                x = 3;
                                numberOfXs = quantity / x;
                                if (quantity > 2)
                                {
                                    double discountAmount = quantity * unitPrice - (numberOfXs * 2 * unitPrice + quantity % 3 * unitPrice);
                                    discount = new Discount(p, "3 for 2", -discountAmount);
                                }
                                break;
                            }
                        case SpecialOfferType.TwoForAmount:
                            {
                                x = 2;
                                numberOfXs = quantity / x;
                                if (quantity >= 2)
                                {
                                    double total = offer.Argument * (quantity / x) + quantity % 2 * unitPrice;
                                    double discountN = unitPrice * quantity - total;
                                    discount = new Discount(p, "2 for " + PrintPrice(offer.Argument), -discountN);
                                }
                                break;
                            }
                        case SpecialOfferType.FiveForAmount:
                            {
                                x = 5;
                                numberOfXs = quantity / x;
                                if (quantity >= 5)
                                {
                                    double discountTotal = unitPrice * quantity - (offer.Argument * numberOfXs + quantity % 5 * unitPrice);
                                    discount = new Discount(p, x + " for " + PrintPrice(offer.Argument), -discountTotal);
                                }
                                break;
                            }
                        case SpecialOfferType.TenPercentDiscount:
                            {
                                discount = new Discount(p, offer.Argument + "% off", -quantity * unitPrice * offer.Argument / 100.0);
                                break;
                            }
                    }

                    if (discount != null)
                        receipt.AddDiscount(discount);

                    //if (offer.OfferType == SpecialOfferType.ThreeForTwo)
                    //{
                    //    x = 3;
                    //}
                    //else if (offer.OfferType == SpecialOfferType.TwoForAmount)
                    //{
                    //    x = 2;
                    //    if (quantity >= 2)
                    //    {
                    //        var total = offer.Argument * (quantity / x) + quantity % 2 * unitPrice;
                    //        var discountN = unitPrice * quantity - total;
                    //        discount = new Discount(p, "2 for " + PrintPrice(offer.Argument), -discountN);
                    //    }
                    //}

                    //if (offer.OfferType == SpecialOfferType.FiveForAmount) x = 5;
                    //var numberOfXs = quantity / x;
                    //if (offer.OfferType == SpecialOfferType.ThreeForTwo && quantity > 2)
                    //{
                    //    var discountAmount = quantity * unitPrice - (numberOfXs * 2 * unitPrice + quantity % 3 * unitPrice);
                    //    discount = new Discount(p, "3 for 2", -discountAmount);
                    //}

                    //if (offer.OfferType == SpecialOfferType.TenPercentDiscount) discount = new Discount(p, offer.Argument + "% off", -quantity * unitPrice * offer.Argument / 100.0);
                    //if (offer.OfferType == SpecialOfferType.FiveForAmount && quantity >= 5)
                    //{
                    //    var discountTotal = unitPrice * quantity - (offer.Argument * numberOfXs + quantity % 5 * unitPrice);
                    //    discount = new Discount(p, x + " for " + PrintPrice(offer.Argument), -discountTotal);
                    //}

                    //if (discount != null)
                    //    receipt.AddDiscount(discount);
                }
            }
        }
        
        private string PrintPrice(double price)
        {
            return price.ToString("N2", Culture);
        }
    }
}