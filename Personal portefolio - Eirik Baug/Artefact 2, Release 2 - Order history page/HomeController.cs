using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.Expressions;
using System.Web.WebPages;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PackageDelivery.Models;
using SendGrid;

namespace PackageDelivery.Areas.Admin.Controllers
{

        /// <summary>
        /// Returns the order history page
        /// </summary>
        /// <param name="search">Search query</param>
        /// <param name="from">From date to check from</param>
        /// <param name="to">To date to check to</param>
        /// <returns>The orders corresponding to the filter.
        /// If no filter is specified return every order in the database.</returns>
        public ActionResult Orders(string search, DateTime? @from, DateTime? to)
        {
            
            //If there is a search, but no to and from date
            if(search != "" && (@from == null || to == null)) { 
                 var results = from m in _context.Packages
                              where
                                  m.OrderId.ToString()==search ||
                                  m.RecieverName.Contains(search) ||
                                  m.Cost.ToString()== search ||
                                  m.SpecialInstructions.Contains(search) ||
                                  m.Weight.ToString() == search ||
                                  m.Order.OrderPriority.ToString() == search ||
                                  m.Order.OrderStatus.ToString() == search ||
                                  m.SenderId == search ||
                                  m.Order.PaymentType.ToString() == search ||
                                  m.Order.ReadyForPickupTime.ToString() == search ||
                                  m.Order.OrderTime.ToString() == search ||
                                  m.Order.PickupAdressId.ToString() == search ||
                                  m.RecieverAdressId.ToString() == search ||
                                  m.PackageId.ToString() == search ||
                                  m.Adress.StreetAdress.Contains(search) ||
                                  m.Adress.Suburb.Contains(search) ||
                                  m.Adress.State.Contains(search) ||
                                  m.Adress.PostCode.ToString()== search ||
                                  m.Order.Adress.StreetAdress.Contains(search) ||
                                  m.Order.Adress.State.Contains(search) ||
                                  m.Order.Adress.Suburb.Contains(search) ||
                                  m.Order.Adress.PostCode.ToString() == search
                              orderby m.OrderId
                              select m;
            
            ViewBag.search = search;
            return View(results);
            }
            //If there is a search and to and from date
            if (search != "" && (@from != null && to != null))
            {
                var results = from m in _context.Packages
                    where
                                 (m.Order.ReadyForPickupTime >= @from &&
                                  m.Order.ReadyForPickupTime <= to) &&(
                                  m.OrderId.ToString() == search ||
                                  m.RecieverName.Contains(search) ||
                                  m.Cost.ToString() == search ||
                                  m.SpecialInstructions.Contains(search) ||
                                  m.Weight.ToString() == search ||
                                  m.Order.OrderPriority.ToString() == search ||
                                  m.Order.OrderStatus.ToString() == search ||
                                  m.SenderId == search ||
                                  m.Order.PaymentType.ToString() == search ||
                                  m.Order.ReadyForPickupTime.ToString() == search ||
                                  m.Order.OrderTime.ToString() == search ||
                                  m.Order.PickupAdressId.ToString() == search ||
                                  m.RecieverAdressId.ToString() == search ||
                                  m.PackageId.ToString() == search ||
                                  m.Adress.StreetAdress.Contains(search) ||
                                  m.Adress.Suburb.Contains(search) ||
                                  m.Adress.State.Contains(search) ||
                                  m.Adress.PostCode.ToString() == search ||
                                  m.Order.Adress.StreetAdress.Contains(search) ||
                                  m.Order.Adress.State.Contains(search) ||
                                  m.Order.Adress.Suburb.Contains(search) ||
                                  m.Order.Adress.PostCode.ToString() == search)

                              orderby m.OrderId
                    select m;
                ViewBag.from = ConvertDateTime(@from);
                ViewBag.to = ConvertDateTime(to);
                ViewBag.search = search;
                return View(results);
            }
            //If there is no surch, but there is a to and from date
            if (search == "" && (@from != null && to != null))
            {
                var results = from m in _context.Packages
                              where
                                  m.Order.ReadyForPickupTime >= @from &&
                                  m.Order.ReadyForPickupTime <= to
                              orderby m.OrderId
                              select m;


                ViewBag.from = ConvertDateTime(@from);
                ViewBag.to = ConvertDateTime(to);
                return View(results);
            }
            //Else just return all the packages in the database
            return View(_context.Packages.ToList());
        }
        /// <summary>
        /// Returns page with details about the order and package
        /// </summary>
        /// <param name="id">Order id</param>
        /// <returns>Page with order/package details.</returns>
        public ActionResult OrderDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Packages package = _context.Packages.Find(id);
            if (package == null)
            {
                return HttpNotFound();
            }

            return View(package);
        }
 