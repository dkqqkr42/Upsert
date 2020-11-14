﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;
using Oracle.ManagedDataAccess.Client;
using BusinessRefinery.Barcode;

namespace FinalProject_Profile
{
    public partial class InspectionDTL : MetroForm
    {
        protected const string connectionString = "DATA SOURCE=220.69.249.228:1521/xe;PASSWORD=1234;PERSIST SECURITY INFO=True;USER ID=MAT_MGR";
        string job_no;

        public InspectionDTL()
        {
            InitializeComponent();
        }
        public InspectionDTL(string _job_no)
        {
            InitializeComponent();
            job_no = _job_no;

            
        }
        protected override CreateParams CreateParams     // 폼 화면 빠른 로딩
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }


        public void FillGrid()  // grd_Result에 값 채우기
        {
            string ct2;

            OracleConnection connection = null;
            try
            {
                ct2 =  "SELECT A.PROD_CODE   PROD_CODE" + Environment.NewLine;
                ct2 += ", B.PROD_NAME   PROD_NAME" +
                       ", E.U_SEQ       U_SEQ" +
                       ", A.PROD_UNIT   PROD_UNIT" +
                       ", A.ORDER_M     ORDER_M" +
                       ", A.JOB_NO      JOB_NO" +
                       ", A.PROC_STATUS PROC_STATUS" +
                       ", A.ORDER_NO || '-' || A.ORDER_SEQ ORDER_NO" +
                       ", C.ROLL_NO     ROLL_NO" +
                       ", C.S_SEQ       S_SEQ" +
                       ", C.SHIFT_CODE  SHIFT_CODE" +
                       ", C.GOOD_QTY + C.BAD_QTY  TUIP_QTY" +
                       ", E.GOOD_QTY    GOOD_QTY" +
                       ", E.BAD_QTY     BAD_QTY" +
                       ", TO_CHAR(E.START_TIME, 'HH24:MI')  SDATE" +
                       ", TO_CHAR(E.END_TIME, 'HH24:MI')    EDATE" +
                       ", C.CONFIRM_FLAG  CONFIRM_FLAG" +
                       ", D.OKBAD_QTY   OKBAD_QTY" +
                       ", C.ETC_QTY     CUTPCS" +
                       ", C.EXT1_QTY    PCSBOX" +
                       ", C.EXT2_QTY    BOXPLT" +
                       ", E.U_SEQ       U_SEQ" +
                       ", E.DEL_FLAG    DEL_FLAG" +
                       ", E.DECISION    DECISION" +
                       ", DECODE(LENGTH(TRIM(NVL(E.BAR_NO, ' '))), 24, 'Y', 'N') BAR_NO" +
                       ", DECODE(LENGTH(TRIM(NVL(E.BAR_NO, ' '))), 24, NVL(E.BAR_NO, ' '), ' ') BAR_NO_STR" +
                       ", E.CONFIRM_FLAG AS CONFIRM_FLAG" +
                       ", E.RAW_FLAG AS RAW_FLAG" +
                       ", E.SCAN_FLAG    AS SCAN_FLAG" +
                       " FROM TBL_PRODUCTPLAN A" +
                       ", TBL_PRODUCTMASTER B" +
                       ", TBL_PRODRSLT C" +
                       ", (SELECT ROLL_NO, S_SEQ, SUM(STD_QTY) OKBAD_QTY" +
                       " FROM TBL_WCDEFECT" +
                       " WHERE(ROLL_NO, S_SEQ) IN(SELECT ROLL_NO, S_SEQ" +
                       " FROM TBL_PRODRSLT" +
                       " WHERE PROD_DATE = :IN_DATE)" +
                       " AND FACTOR_CODE <> 'XXX'" +
                       " AND DEL_FLAG = 'A'" +
                       " GROUP BY ROLL_NO," +
                       "S_SEQ) D" +
                       ", TBL_PRODRSLT_DTL E " +
                       " WHERE A.JOB_NO = C.JOB_NO" +
                       " AND A.PROD_CODE = B.PROD_CODE" +
                       " AND C.ROLL_NO = E.ROLL_NO" +
                       " AND C.S_SEQ = E.S_SEQ" +
                       " AND(C.ROLL_NO = D.ROLL_NO(+) AND C.S_SEQ = D.S_SEQ(+))" +
                       " AND C.PROD_DATE = :IN_DATE" +
                       " AND A.PLANT_CODE = '2020'" +
                    //   " AND C.JOB_NO = '"+ job_no +"'" +
                       " ORDER BY C.START_TIME";

                connection = new OracleConnection
                {
                    ConnectionString = connectionString
                };
                connection.Open();

                OracleCommand cmd = new OracleCommand
                {
                    CommandType = CommandType.Text,
                    Connection = connection,
                    CommandText = ct2
                };

                cmd.Parameters.Add("IN_DATE", dtp_DATE.Value.ToString("yyyyMMdd"));  // sql문 IN_DATE에 dtp_DATE의 값을 넣어줌

                OracleDataAdapter adapter = new OracleDataAdapter(cmd);

                DataSet ds = new DataSet();
                adapter.Fill(ds);
                DataTable dt = ds.Tables[0];
                grd_Result.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
            }
        }



        private void metroButton3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void grd_Result_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 &&
            grd_Result.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected)
            {
                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
            }
            else
            {
                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Regular);
            }
        }

        private void InspectionDTL_Load(object sender, EventArgs e)  // InspectionDTL 폼이 켜질 때
        {
            FillGrid();
        }

        private void InspectionDTL_Activated(object sender, EventArgs e)  // InspectionDTL 폼이 활성화 될 때
        {
            FillGrid();
        }

        private void dtp_DATE_ValueChanged(object sender, EventArgs e)  // dtp_DATE 날짜 변경했을 때
        {
            FillGrid();
        }

        private void btn_Barcode_Click(object sender, EventArgs e)  // 바코드 발행 버튼 눌렀을 때 실행
        {
            // SCAN_FLAG 값을 N -> Y 로 변경
            OracleConnection connection = null;
            try
            {
                int rowIndex = grd_Result.CurrentRow.Index;  // 선택된 행을 가져옴
                connection = new OracleConnection
                {
                    ConnectionString = connectionString
                };
                connection.Open();

                OracleCommand cmd = new OracleCommand
                {
                    CommandType = CommandType.Text,
                    Connection = connection,
                    CommandText = "UPDATE TBL_PRODRSLT_DTL SET SCAN_FLAG = 'Y' where ROLL_NO = :IN_ROLL_NO"
                };
                cmd.Parameters.Add("IN_ROLL_NO", grd_Result.Rows[rowIndex].Cells[7].Value.ToString());
                cmd.ExecuteNonQuery();
                MessageBox.Show("바코드를 발행하였습니다.", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);






            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
            }

            // 바코드 생성
            QRCode barcode = new QRCode();
            string url = "20201114";
            barcode.Code = url;
            barcode.ModuleSize = 6.0f;  // 바코드의 크기
            barcode.Resolution = 300;   // 바코드의 해상도
            barcode.drawBarcode2ImageFile("barcode.png");
            // barcode.Format = ImageFormat.Gif;
            // barcode.drawBarcode2ImageFile("c:/qr-code-csharp.gif");  // 저장위치, 파일이름

        }
    }
}
