using System;
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
            foreach (Product p in _productQuantities.Keys)
            {
                double quantity = _productQuantities[p];
                int quantityFloored = (int)quantity;

                if (offers.ContainsKey(p))
                {
                    Offer offer = offers[p];
                    double unitPrice = catalog.GetUnitPrice(p);
                    double originalPrice = quantity * unitPrice;
                    double discountedAmount = 0;
                    Discount discount = null;

                    switch (offer.OfferType)
                    {
                        case SpecialOfferType.TwoForAmount:
                            {
                                if (quantity >= 2)
                                {
                                    discountedAmount = DiscountedAmountXForY(
                                                           originalPrice,
                                                           unitPrice,
                                                           quantityFloored,
                                                           2,/*for*/offer.Argument);

                                    discount = new Discount(p, "2 for " + PrintPrice(offer.Argument), -discountedAmount);
                                }
                                break;
                            }
                        case SpecialOfferType.ThreeForTwo:
                            {
                                if (quantity >= 3)
                                {
                                    discountedAmount = DiscountedAmountXForY(
                                                           originalPrice,
                                                           unitPrice,
                                                           quantityFloored,
                                                           3,/*for*/2 * unitPrice);

                                    discount = new Discount(p, "3 for 2", -discountedAmount);
                                }
                                break;
                            }
                        case SpecialOfferType.FiveForAmount:
                            {
                                if (quantity >= 5)
                                {
                                    discountedAmount = DiscountedAmountXForY(
                                                           originalPrice,
                                                           unitPrice,
                                                           quantityFloored,
                                                           5,/*for*/offer.Argument);

                                    discount = new Discount(p, 5 + " for " + PrintPrice(offer.Argument), -discountedAmount);
                                }
                                break;
                            }
                        case SpecialOfferType.TenPercentDiscount:
                            {
                                discountedAmount = originalPrice * offer.Argument / 100.0;

                                discount = new Discount(p, offer.Argument + "% off", -discountedAmount);
                                break;
                            }
                    }

                    if (discount != null)
                        receipt.AddDiscount(discount);
                }
            }
        }
        
        private string PrintPrice(double price)
        {
            return price.ToString("N2", Culture);
        }

        private double DiscountedAmountXForY(
            double originalPrice,
            double unitPrice,
            int quantityFloored,
            int x,/*for*/double y)
        {
            double discountedPrice = (quantityFloored / x) * y
                                   + (quantityFloored % x) * unitPrice;

            return originalPrice - discountedPrice;
        }
    }
}