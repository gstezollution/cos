﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Data;
using System.Net;
//for add color in grideview
using System.Drawing;

public partial class Admin_OmgTransaction : AdminBasePage, ITransactionFeedEntryView
{

    private TransactionFeedPrensenter _presenter;
    public Admin_OmgTransaction()
    {
        this._presenter = new TransactionFeedPrensenter(this);
    }


    DataTable dt = new DataTable();
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            TransFromDB();
        }
    }

    private void TransFromDB()
    {
        gvItems.DataSource = Transaction_Feeds;
        gvItems.DataBind();
        if (gvItems.Rows.Count > 0)
        {
            gvItems.UseAccessibleHeader = true;
            gvItems.HeaderRow.TableSection = TableRowSection.TableHeader;
            gvItems.FooterRow.TableSection = TableRowSection.TableFooter;
        }
        
    }
    private void getTransactionDetails()
    {
        DateTime dateFrom = Convert.ToDateTime(txtFromdate.Text);
        string dayFrom = dateFrom.Day.ToString();
        string monthFrom = dateFrom.Month.ToString();
        string yearFrom = dateFrom.Year.ToString();

        DateTime dateTo = Convert.ToDateTime(txtTodate.Text.Trim());
        string dayTo = dateTo.Day.ToString();
        string monthTo = dateTo.Month.ToString();
        string yearTo = dateTo.Year.ToString();


        string url = "https://admin.omgpm.com/v2/reports/affiliate/leads/leadsummaryexport.aspx?Contact=764019&Country=26&Agency=95&Status=-1&Year=" + yearFrom + "&Month=" + monthFrom + "&Day=" + dayFrom + "&EndYear=" + yearTo + "&EndMonth=" + monthTo + "&EndDay=" + dayTo + "&DateType=0&Sort=CompletionDate&Login=31600E6D4717172FC8C62F9C1D35CE11&Format=XML&RestrictURL=0";

        XElement xele = XElement.Load(url);
        dt = clsFileToTable.XElementToDataset(xele).Tables["Report_Details_Group"];
       
        if (dt.Rows.Count > 0)
        {
            AssignDataToDbParameter(dt);
        }
    }

    protected void btnPushToDB_Click(object sender, EventArgs e)
    {
        EventHandler Insert = this.InsertCommand;
        if (Insert != null)
        {
            getTransactionDetails();
            //lblMessage.Text = string.Empty;
            Insert(this, e);

            clsProcCallingService.Proc_OmgTransactionFinalCommision();
          
        }
        TransFromDB();
    }
    protected void lkbUpdateValidate_Click(object sender, EventArgs e)
    {
        List<Int64> offerIds = new List<Int64>();
        //decimal FinalCommision;
        List<Transaction_Feed> feeds = new List<Transaction_Feed>();
        foreach (GridViewRow row in gvItems.Rows)
        {
            if (((CheckBox)row.FindControl("chkRow")).Checked)
            {
                Transaction_Feed item = new Transaction_Feed();
                Int64 offerID = Convert.ToInt32(gvItems.DataKeys[row.RowIndex].Value);
                offerIds.Add(offerID);
            }
        }

        clsCommonMethods obj = new clsCommonMethods();
        obj.UpdateTransactionFeed(offerIds,Convert.ToInt16(Constants.AmountStatus.Validated));
        lblMessage.Text = "Validate User amount.";
    }
    protected void lkbUpdate_Click(object sender, EventArgs e)
    {
        List<Int64> offerIds = new List<Int64>();
        //decimal FinalCommision;
        List<Transaction_Feed> feeds = new List<Transaction_Feed>();
        foreach (GridViewRow row in gvItems.Rows)
        {
            if (((CheckBox)row.FindControl("chkRow")).Checked)
            {
                Transaction_Feed item = new Transaction_Feed();
                Int64 offerID = Convert.ToInt32(gvItems.DataKeys[row.RowIndex].Value);
                item.OmgTransactionID = gvItems.DataKeys[row.RowIndex]["OmgTransactionID"].ToString();

                item.OmgMerchantRef = gvItems.DataKeys[row.RowIndex]["omgmerchantref"].ToString();
                item.MerchantName = gvItems.DataKeys[row.RowIndex]["MerchantName"].ToString();
                item.UserEmail = gvItems.DataKeys[row.RowIndex]["UserEmail"].ToString();

                if (!string.IsNullOrEmpty(((TextBox)row.FindControl("txtFinalCommision")).Text))
                    item.FinalCommision = Convert.ToDecimal(((TextBox)row.FindControl("txtFinalCommision")).Text);


                
                feeds.Add(item);
                offerIds.Add(offerID);
                if (!string.IsNullOrEmpty(item.UserEmail))
                    SendEmail(item.UserEmail, item.FinalCommision.ToString(), item.OmgMerchantRef.ToString(), item.MerchantName);

            }
        }

         EventHandler Insert = this.InsertCommand;
         if (Insert != null)
         {
             Transaction_FeedInsert = feeds;
             //lblMessage.Text = string.Empty;
             Insert(this, e);

            // TransFromDB();
         }

        if (offerIds.Count > 0)
        {
            EventHandler update = this.UpdateCommand;
            if (update != null)
            {
                this.Ids = offerIds;
                //lblMessage.Text = string.Empty;
                update(this, e);
                TransFromDB();
            }
        }
    }


    private void SendEmail(string toUserEmail,string rs, string orderID, string MerchantName)
    {
        string body = clsEmailMailer.PopulateAddCashBackBody(rs, orderID, MerchantName);
        clsEmailMailer.SendHtmlFormattedEmail(toUserEmail, "Rs. " + rs + " added as cash back in CashONshop wallet.", body);
    }


    protected void Delete_Click(object sender, EventArgs e)
    {
        List<Int64> offerIds = new List<Int64>();
        foreach (GridViewRow row in gvItems.Rows)
        {
            if (((CheckBox)row.FindControl("chkRow")).Checked)
            {

                Int64 offerID = Convert.ToInt32(gvItems.DataKeys[row.RowIndex].Value);
                offerIds.Add(offerID);
            }
        }

        if (offerIds.Count > 0)
        {
            EventHandler delete = this.DeleteCommand;
            if (delete != null)
            {
                this.Ids = offerIds;
                //lblMessage.Text = string.Empty;
                delete(this, e);
                TransFromDB();
            }
        }
    }
    protected void btnMerchantNameAdd_Click(object sender, EventArgs e)
    {

    }

    private void AssignDataToDbParameter(DataTable dt)
    {
      
        //Transaction_Feeds
        List<Transaction_Feed> feeds = new List<Transaction_Feed>();
        foreach (DataRow row in dt.Rows)
        {
            Transaction_Feed item = new Transaction_Feed();
            item.ClickTime = row["ClickTime"].ToString();
            
            item.TransactionTime = Convert.ToDateTime(row["TransactionTime"].ToString());
            item.OmgTransactionID = row["TransactionID"].ToString();
            item.OmgMerchantRef = row["MerchantRef"].ToString();
            if (dt.Columns.Contains("UID"))
                item.UID = row["UID"].ToString();
            //item.UID2 = row["UID2"].ToString();
            item.MID = row["MID"].ToString();
            //item.MerchantName = row["MerchantName"].ToString();
            item.PID = row["PID"].ToString();
            item.Product = row["Product"].ToString();
            item.Referrer = row["Referrer"].ToString();
            item.SR = row["SR"].ToString();
            item.VR = row["VR"].ToString();
            item.NVR = row["NVR"].ToString();
            item.Status = row["Status"].ToString();
            item.Paid = row["Paid"].ToString();
            if (dt.Columns.Contains("Completed"))
                item.Completed = row["Completed"].ToString();

            item.UKey = row["UKey"].ToString();
            item.TransactionValue = row["TransactionValue"].ToString();
            if (dt.Columns.Contains("VoucherCode"))
                item.VoucherCode = row["VoucherCode"].ToString();
            feeds.Add(item);
        }

        Transaction_FeedInsert = feeds;

    }

    public string ClickTime { get; set; }
    public string TransactionTime { get; set; }
    public string OmgTransactionID { get; set; }
    public string OmgMerchantRef { get; set; }
    public string UID { get; set; }
    public string UID2 { get; set; }
    public string MID { get; set; }
    public string MerchantName { get; set; }
    public string PID { get; set; }
    public string Product { get; set; }
    public string Referrer { get; set; }
    public string SR { get; set; }
    public string VR { get; set; }
    public string NVR { get; set; }
    public string Status { get; set; }
    public string Paid { get; set; }
    public string Completed { get; set; }
    public string UKey { get; set; }
    public string TransactionValue { get; set; }
    public string VoucherCode { get; set; }
    public string FromDate { get; set; }

    public string ToDate { get; set; }
    public string AmountStatus { get; set; }

    public string strMessage
    {
        set { lblMessage.Text = value; }
    }

    public List<long> Ids { get; set; }
    public event EventHandler InsertCommand;
    public event EventHandler DeleteCommand;
    public event EventHandler UpdateCommand;

    public List<Transaction_Feed> Transaction_Feeds { get; set; }



    public List<Transaction_Feed> Transaction_FeedInsert { get; set; }






   

    public event EventHandler SearchCommand;
    protected void btnSearchTransaction_Click(object sender, EventArgs e)
    {
        EventHandler search = this.SearchCommand;
        if (search != null)
        {
            this.FromDate = txtFromSearchDate.Text.ToString();
            this.ToDate = txtToSearchDate.Text.ToString();
            this.AmountStatus = ddlStatus.SelectedItem.Text.ToString();
            search(this, e);
            TransFromDB();
        }
    }

    //Apply Color on Grideview when final comm. is null
    protected void gvItems_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            TextBox txt = ((TextBox)e.Row.FindControl("txtFinalCommision"));

             if (string.IsNullOrEmpty(txt.Text))
             {
                foreach (TableCell cell in e.Row.Cells)
                {
                        cell.BackColor = Color.Red;
                }
             }
        }
    }
}