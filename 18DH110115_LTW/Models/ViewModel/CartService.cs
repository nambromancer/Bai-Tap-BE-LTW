using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _18DH110115_LTW.Models.ViewModel
{
    public class CartService
    {
        public readonly HttpSessionStateBase session;

        public CartService(HttpSessionStateBase session)
        {
            this.session = session;
        }

        public Cart GetCart()
        {
            var cart = (Cart)session["Cart"];
            if (cart == null)
            {
                cart = new Cart();
                session["Cart"] = cart;
            }
            return cart;
        }

        public void ClearCart()
        {
            session["Cart"] = null;
        }
    }
}