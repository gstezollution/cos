﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class UC_UCUI_Product_uc_OfferCommonUI : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    public List<Merchant_Offer> Offers
    {
        set
        {
            rpItems.DataSource = value;
            rpItems.DataBind();
        }
    }
    public List<Merchant_Offer> Offers1
    {
        set
        {
            rpItems1.DataSource = value;
            rpItems1.DataBind();
        }
    }
    public List<Merchant_Offer> Offers2
    {
        set
        {
            rpItems2.DataSource = value;
            rpItems2.DataBind();
        }
    }
    public string ReduceString(string obj)
    {
        return obj.Truncate(40, "...");
    }
}