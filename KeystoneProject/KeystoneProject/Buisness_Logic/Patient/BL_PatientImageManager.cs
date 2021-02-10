﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using KeystoneProject.Models.Patient;
using KeystoneProject.Buisness_Logic.Hospital;
using System.Configuration;
using Microsoft.ApplicationBlocks.Data;
using Microsoft.ApplicationBlocks.ExceptionManagement;

namespace KeystoneProject.Buisness_Logic.Patient
{
    public class BL_PatientImageManager
    {
        private SqlConnection con;
        int HospitalID = Convert.ToInt32(HttpContext.Current.Session["HospitalID"]);
        int LocationID = Convert.ToInt32(HttpContext.Current.Session["LocationID"]);
        int UserID = Convert.ToInt32(HttpContext.Current.Session["UserID"]);

        List<PatientImageManager> PatientImageManagerList = new List<PatientImageManager>();
        PatientImageManager obj = new PatientImageManager();
        BL_HospitalLocation objEmail = new BL_HospitalLocation();

        private void Connect()
        {
            string Constring = ConfigurationManager.ConnectionStrings["Mycon"].ToString();
            con = new SqlConnection(Constring);
        }
        public List<PatientImageManager> BindPatient(string PatientName )
        {
            Connect();
            SqlCommand cmd = new SqlCommand("select PatientRegNO ,PatientType, PatientName +' '+ PMiddleName +' '+ PLastName as 'PatientName' ,(Age/365) as Age,GuardianName,Gender,Address,MobileNo from Patient where Patient.HospitalID = " + HospitalID + "  and Patient.LocationID =" + LocationID + " and Patient.RowStatus= 0 and  Patient.PatientName like '" + PatientName + "%' ORDER BY CONVERT(int, Patient.PatientRegNO) Desc", con);
            DataSet ds = new DataSet();
            con.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            foreach (DataRow dr in dt.Rows)
            {
                //if (dr["DateOfBirth"].ToString() != "")
                //{
                    PatientImageManagerList.Add(
                        new PatientImageManager
                        {
                            PatientRegNo = Convert.ToInt32(dr["PatientRegNo"]),
                           // OPDIPDID=Convert.ToInt32(dr[""]),
                            PatientName = (dr["PatientName"]).ToString(),
                            GuardianName = (dr["GuardianName"]).ToString(),
                            Gender = (dr["Gender"]).ToString(),
                            Age = Convert.ToInt32(dr["Age"]),
                            PatientType = (dr["PatientType"]).ToString(),
                            Address = (dr["Address"]).ToString(),
                            MobileNo = (dr["MobileNo"]).ToString(),
                            
                            
                        
                           // Height = Convert.ToInt32(dr["Height"]),
                            //FinancialYearID = Convert.ToInt32(dr["FinancialYearID"])


                            

                        });
                //}
            }


            return PatientImageManagerList;

        }

        public bool Save(PatientImageManager obj)
        {
            // string id = obj.PatientRegNO.ToString();
            try
            {

                Connect();


                #region IUPatientImageManager

                //string[] Paper = obj.Paper.Split(',');
                string[] Papername = obj.PaperPath.Split(',');

                for (int i = 0; i < Papername.Length; i++)
                {

                    SqlCommand cmPatientMRD = new SqlCommand("IUPatientImageManager", con);
                    cmPatientMRD.CommandType = CommandType.StoredProcedure;

                    cmPatientMRD.Parameters.AddWithValue("@HospitalID", HospitalID);
                    cmPatientMRD.Parameters.AddWithValue("@LocationID", LocationID);
                    cmPatientMRD.Parameters.AddWithValue("@CasePaperID", 0);
                    cmPatientMRD.Parameters["@CasePaperID"].Direction = ParameterDirection.Output;


                    cmPatientMRD.Parameters.AddWithValue("@PatientRegNO", obj.PatientRegNo);
                    cmPatientMRD.Parameters.AddWithValue("@OPDIPDID", obj.OPDIPDID);
                    //cmPatientMRD.Parameters.AddWithValue("@PatientIPDNO", obj.OPDIPDID);
                    cmPatientMRD.Parameters.AddWithValue("@PatientType", obj.PatientType);

                    cmPatientMRD.Parameters.AddWithValue("@PaperType", "");
                    cmPatientMRD.Parameters.AddWithValue("@Paper", "");
                    if (Papername[i] == null)
                    {
                        cmPatientMRD.Parameters.AddWithValue("@PaperPath", "");
                    }
                    else
                    {
                        cmPatientMRD.Parameters.AddWithValue("@PaperPath", Papername[i]);
                    }
                   
                    cmPatientMRD.Parameters.AddWithValue("@PaperName", Papername[i].Replace("/MRDFiles/", ""));

                    cmPatientMRD.Parameters.AddWithValue("@ConsultantDrID", 0);
                    cmPatientMRD.Parameters.AddWithValue("@ConsultantDrName", "");
                    cmPatientMRD.Parameters.AddWithValue("@ReferredDrID", 0);
                    cmPatientMRD.Parameters.AddWithValue("@ReferredDrName", "");

                    if (obj.CreationID != null)
                    {
                        cmPatientMRD.Parameters.AddWithValue("@CreationID", UserID);
                    }
                    else
                    {
                        cmPatientMRD.Parameters.AddWithValue("@CreationID", UserID);

                    }

                    cmPatientMRD.Parameters.AddWithValue("@Mode", "Add");
                    con.Open();
                    cmPatientMRD.ExecuteNonQuery();
                    con.Close();

                    int CasePaperID = Convert.ToInt32(cmPatientMRD.Parameters["@CasePaperID"].Value.ToString());
                #endregion

                }
            }
            catch (Exception ex)
            {

            }
            return true;


        }

        public string DeletePatientImageManager(int CasePaperID)
        {
            string Table = string.Empty;
            Connect();
            SqlCommand cmd = new SqlCommand("DeletePatientImageManager", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CasePaperID", CasePaperID);
            cmd.Parameters.AddWithValue("@LocationID", LocationID);
            cmd.Parameters.AddWithValue("@HospitalID", HospitalID);
            con.Open();
           
            int i = cmd.ExecuteNonQuery();
            con.Close();

            if (i > 0)
            {
                return Table;
            }
            else
            {
                return Table;
            }

        }



        public DataSet GetHospitalLocationData(int LocationID)
        {
             DataSet ds = new DataSet();
            try
            {
                Connect();
                SqlParameter[] param = new SqlParameter[1];
                param[0] = new SqlParameter("@LocationID", SqlDbType.Int);
                param[0].Value = LocationID;
                //param[1] = new SqlParameter("@LocationName", SqlDbType.VarChar);
                //param[1].Value = LocationName;
               ds= SqlHelper.ExecuteDataset(con, CommandType.StoredProcedure, "GetHospitalLocation", param);
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);
                throw ex;
            }
            return ds;

        }
        public DataSet GetHospital(int HospitalID,int LocationID)
        {
              DataSet ds = new DataSet();
            try
            {
                Connect();

                SqlParameter[] param = new SqlParameter[3];
                param[0] = new SqlParameter("@HospitalID", SqlDbType.Int);
                param[0].Value = HospitalID;
                param[1] = new SqlParameter("@LocationID", SqlDbType.Int);
                param[1].Value = LocationID;
                ds = SqlHelper.ExecuteDataset(con, CommandType.StoredProcedure, "GetHospital", param);

            }

            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);
                throw ex;
            }
            return ds;
        }


       
        public DataSet GetPatientImageManager(int HospitalID, int LocationID, int PatientRegNo)
        {
            DataSet ds = new DataSet();
            try
            {
                Connect();
                SqlParameter[] param = new SqlParameter[3];
                param[0] = new SqlParameter("@HospitalID", SqlDbType.Int);
                param[0].Value = HospitalID;
                param[1] = new SqlParameter("@LocationID", SqlDbType.Int);
                param[1].Value = LocationID;
                param[2] = new SqlParameter("@PatientRegNO", SqlDbType.Int);
                param[2].Value = PatientRegNo;
                ds = SqlHelper.ExecuteDataset(con, CommandType.StoredProcedure, "GetPatientImageManager", param);
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);
                throw ex;
            }
            return ds;
        }


    }
}