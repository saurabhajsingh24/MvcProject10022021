﻿using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;
namespace KeystoneProject.Report
{
    public partial class RptPatientLabBills1 : System.Web.UI.Page
    {
        ReportDocument rd = new ReportDocument();
        ReportDocument rd1 = new ReportDocument();
        protected void Page_Load(object sender, EventArgs e)
        {
            int HospitalID = Convert.ToInt32(Session["HospitalID"]);
            int LocationID = Convert.ToInt32(Session["LocationID"]);

            string LabBillType = Session["LabBillType"].ToString();
            int LabBillNo = Convert.ToInt32(Session["LabBillNo"]);
            SqlConnection con;
            string constring = ConfigurationManager.ConnectionStrings["Mycon"].ToString();
            con = new SqlConnection(constring);
             DataSet ds = new DataSet();

             SqlCommand cmd = new SqlCommand("RptPatientLabBills", con);
             cmd.CommandType = CommandType.StoredProcedure;
             cmd.CommandTimeout = 500;
             cmd.Parameters.AddWithValue("@HospitalID", HospitalID);
             cmd.Parameters.AddWithValue("@LocationID", LocationID);
             cmd.Parameters.AddWithValue("@BillType", LabBillType);
             cmd.Parameters.AddWithValue("@BillNo", LabBillNo);
             SqlDataAdapter da = new SqlDataAdapter(cmd);

             //con.Open();
             da.Fill(ds);

             List<byte[]> files = new List<byte[]>();
             DataSet paymentRecipt = RptPaymentRecipt(ds, HospitalID, LocationID);


             if (ds.Tables[0].Rows.Count != 0)
             {


                 string ServiceName = "";

                 String str = "";

                 rd.Load(Path.Combine(Server.MapPath("~/Report"), "RptPatientLabBills.rpt"));
                 rd.SetDataSource(ds);
                 Stream stream = rd.ExportToStream(ExportFormatType.PortableDocFormat);

                 files.Add(PrepareBytes(stream));

                 //rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, Response, false, "Crystal.pdf");
                 //--------------------------------------Payment Report 
                 if (paymentRecipt.Tables[1].Rows.Count > 0)
                 {
                     if (Convert.ToInt32(paymentRecipt.Tables[1].Rows[0]["CrAmount"]) > 0)
                     {
                         rd1.Load(Path.Combine(Server.MapPath("~/Report"), "RptPreBalanceAmount.rpt"));

                         rd1.SetDataSource(paymentRecipt);

                         Stream stream1 = rd1.ExportToStream(ExportFormatType.PortableDocFormat);
                         files.Add(PrepareBytes(stream1));
                     }
                     //  rd1.ExportToHttpResponse(ExportFormatType.PortableDocFormat, Response, false, "Crystal.pdf");

                     //   Response.End();
                 }


             }
             Response.Clear();
             Response.Buffer = true;
             Response.ContentType = "application/pdf";

             //merge the all reports & show the reports            
             Response.BinaryWrite(MergeReports(files).ToArray());

             Response.End();  









                
        }

        public DataSet RptPaymentRecipt(DataSet ds1, int strHospital, int strLocation)
        {
            string LabBillType = Session["LabBillType"].ToString();
            SqlConnection con;
            string constring = ConfigurationManager.ConnectionStrings["Mycon"].ToString();
            con = new SqlConnection(constring);

            SqlCommand cmd = new SqlCommand("RptBalanceAmount", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 500;
            cmd.Parameters.AddWithValue("@HospitalID", strHospital);
            cmd.Parameters.AddWithValue("@LocationID", strLocation);
            cmd.Parameters.AddWithValue("@BillNo", ds1.Tables[1].Rows[0]["BillNo"].ToString());
            cmd.Parameters.AddWithValue("@BillType", LabBillType);
            cmd.Parameters.AddWithValue("@FinancialYearID",2);
            //cmd.Parameters.AddWithValue("@BillNo", 864);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            //con.Open();
            da.Fill(ds);

            return ds;
        }
        private byte[] PrepareBytes(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer = new byte[stream.Length];
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private MemoryStream MergeReports(List<byte[]> files)
        {
            if (files.Count > 1)
            {
                PdfReader pdfFile;
                Document doc;
                PdfWriter pCopy;
                MemoryStream msOutput = new MemoryStream();

                pdfFile = new PdfReader(files[0]);

                doc = new Document();
                pCopy = new PdfSmartCopy(doc, msOutput);

                doc.Open();

                for (int k = 0; k < files.Count; k++)
                {
                    pdfFile = new PdfReader(files[k]);
                    for (int i = 1; i < pdfFile.NumberOfPages + 1; i++)
                    {
                        ((PdfSmartCopy)pCopy).AddPage(pCopy.GetImportedPage(pdfFile, i));
                    }
                    pCopy.FreeReader(pdfFile);
                }

                pdfFile.Close();
                pCopy.Close();
                doc.Close();

                return msOutput;
            }
            else if (files.Count == 1)
            {
                return new MemoryStream(files[0]);
            }

            return null;
        }
        protected void Page_Unload(object sender, EventArgs e)
        {
            if (this.rd != null)
            {
                this.rd.Close();
                this.rd.Dispose();
            }
        }
    }
}