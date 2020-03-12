using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSTrader;
using CSAPIComm;


namespace TradeAPIExample
{
    public partial class TradeAPIExampleForm : Form
    {

        private TradeAPI tradeAPI = null;

        public TradeAPIExampleForm()
        {
            InitializeComponent();

            tbxFFutures7.Text = DateTime.Now.ToString("yyyyMM");
            textBox1.Text = DateTime.Now.ToString("yyyyMMdd");
            textBox2.Text = DateTime.Now.ToString("yyyyMMdd");
            textBox3.Text = DateTime.Now.ToString("yyyyMMdd");
            textBox4.Text = DateTime.Now.ToString("yyyyMMdd");
            textBoxQueryStartDate.Text = DateTime.Now.ToString("yyyyMMdd");
            textBoxQueryEndDate.Text = DateTime.Now.ToString("yyyyMMdd");
        }

        private void setButtons(int status)
        {
            switch (status)
            {
                case 1: // 未連線/連線錯誤/已斷線
                    btnConnect.Enabled = true;
                    btnDisconnect.Enabled = false;
                    btnTradeLogin.Enabled = false;
                    btnWinLoginQ.Enabled = false;
                    btnDestroy.Enabled = true;
                    chkAutoLoadProduct.Enabled = true;
                    chkSubReports.Enabled = true;
                    chkRecoverReports.Enabled = true;
                    break;
                case 2: // 連線中/斷線中
                    btnConnect.Enabled = false;
                    btnDisconnect.Enabled = false;
                    btnTradeLogin.Enabled = false;
                    btnWinLoginQ.Enabled = false;
                    btnDestroy.Enabled = false;
                    chkAutoLoadProduct.Enabled = false;
                    chkSubReports.Enabled = false;
                    chkRecoverReports.Enabled = false;
                    break;
                case 3: // 連線成功
                    btnConnect.Enabled = false;
                    btnDisconnect.Enabled = true;
                    btnTradeLogin.Enabled = true;
                    btnWinLoginQ.Enabled = true;
                    btnDestroy.Enabled = true;
                    chkAutoLoadProduct.Enabled = true;
                    chkSubReports.Enabled = true;
                    chkRecoverReports.Enabled = true;
                    break;
                case 4: // 連線成功, 登入成功
                    btnConnect.Enabled = false;
                    btnDisconnect.Enabled = true;
                    btnTradeLogin.Enabled = false;
                    btnWinLoginQ.Enabled = false;
                    btnDestroy.Enabled = true;
                    chkAutoLoadProduct.Enabled = false;
                    chkSubReports.Enabled = false;
                    chkRecoverReports.Enabled = false;
                    break;
                case 5: // 連線成功, 登入失敗
                    btnConnect.Enabled = false;
                    btnDisconnect.Enabled = true;
                    btnTradeLogin.Enabled = true;
                    btnWinLoginQ.Enabled = true;
                    btnDestroy.Enabled = true;
                    chkAutoLoadProduct.Enabled = true;
                    chkSubReports.Enabled = true;
                    chkRecoverReports.Enabled = true;
                    break;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (tradeAPI == null)
            {
                tradeAPI = new TradeAPI(cbQuoteHost.Text, ushort.Parse(tbQuotePort.Text), tbSID.Text);
                tradeAPI.OnTradeAPIRcvData += OnTAPIRcvData;
                tradeAPI.OnTradeAPIStatus += OnTAPIStatus;
            }
            int res = tradeAPI.Connect();
            if (res < 0)
                setButtons(1);
        }

        private void OnTAPIRcvData(TradeAPI sender, MomBase mb)
        {
            switch (mb.dataType)
            {
                case 103:
                    MB103 mb103 = mb as MB103;
                    rptBox.AppendText(string.Format("公告查詢成功，數量[{0}]", mb103.notices.Count));
                    rptBox.AppendText(Environment.NewLine);
                    foreach (NoticeMsg item in mb103.notices)
                    {
                        rptBox.AppendText(string.Format("{0}|{1}|{2}|{3}", item.kind, item.notice_type, item.post_time, item.content));
                        rptBox.AppendText(Environment.NewLine);
                    }
                    break;
                case 114:
                    MB114 mb114 = mb as MB114;
                    rptBox.AppendText(string.Format(" 收到主機公告, {0}", mb114.content));
                    rptBox.AppendText(Environment.NewLine);
                    break;
                case 201:
                    {
                        MB201 mb201 = mb as MB201;
                        int errorCode = 0;
                        if (int.TryParse(mb201.err_code, out errorCode) && (errorCode == 0))
                            rptBox.AppendText(string.Format("{0} 委託成功 {1}", mb201.user_def, mb201.toLog()));
                        else
                            rptBox.AppendText(string.Format("{0} 委託失敗, ErrCode={1} ErrMsg={2}", mb201.user_def, mb201.err_code, mb201.err_msg));
                        rptBox.AppendText(Environment.NewLine);
                    }
                    break;
                case 202:
                    MB202 mb202 = mb as MB202;
                    rptBox.AppendText(string.Format("{0} 成交{1}口 {2}", mb202.user_def, mb202.qty_cum, mb202.toLog()));
                    rptBox.AppendText(Environment.NewLine);
                    break;
                case 401:
                    {
                        MB401 mb401 = mb as MB401;
                        int errorCode = 0;
                        if (int.TryParse(mb401.err_code, out errorCode) && (errorCode == 0))
                            rptBox.AppendText(string.Format("{0} 證券委託成功 {1}", mb401.user_def, mb401.toLog()));
                        else
                            rptBox.AppendText(string.Format("{0} 證券委託失敗, ErrCode={1} ErrMsg={2}", mb401.user_def, mb401.err_code, mb401.err_msg));
                        rptBox.AppendText(Environment.NewLine);
                    }
                    break;
                case 402:
                    MB402 mb402 = mb as MB402;
                    rptBox.AppendText(string.Format("{0} 成交{1}口 {2}", mb402.user_def, mb402.deal_qty, mb402.toLog()));
                    rptBox.AppendText(Environment.NewLine);
                    break;
                case 404:
                case 405:
                case 406:
                    rptBox.AppendText(string.Format(" 委託回報回補:{0}", mb.toLog()));
                    rptBox.AppendText(Environment.NewLine);
                    break;
                case 411:
                    resultBox.AppendText(string.Format(" 即時庫存查詢:{0}", mb.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 413:
                    resultBox.AppendText(string.Format(" 對帳單查詢:{0}", mb.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 415:
                    resultBox.AppendText(string.Format(" 整戶維持率查詢:{0}", mb.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 417:
                    resultBox.AppendText(string.Format(" 證券庫存回報:{0}", mb.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 419:
                    resultBox.AppendText(string.Format(" 證券全額交割股回覆:{0}", mb.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 421:
                    resultBox.AppendText(string.Format(" 證券歷史淨收付回報:{0}", mb.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 423:
                    resultBox.AppendText(string.Format(" 證券對帳單回報:{0}", mb.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 425:
                    resultBox.AppendText(string.Format(" 證券已實現損益查詢:{0}", mb.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 427:
                    resultBox.AppendText(string.Format(" 證券即時庫存明細損益試算回報:{0}", mb.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 429:
                    resultBox.AppendText(string.Format(" 證券即時庫存彙總損益試算回報:{0}", mb.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 431:
                    resultBox.AppendText(string.Format(" 證券自訂成本回覆:{0}", mb.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 205:
                    MB205 mb205 = mb as MB205;
                    if (mb205.sub_acno.Trim().Length > 0)
                        rptBox.AppendText(string.Format("{0}-{1} 委託回報回補 {2}", mb205.branch_id, mb205.sub_acno, mb205.toLog()));
                    else
                        rptBox.AppendText(string.Format("\r\n{0}-{1} 委託回報回補 {2}", mb205.branch_id, mb205.acno, mb205.toLog()));
                    rptBox.AppendText(Environment.NewLine);
                    break;
                case 206:
                    MB206 mb206 = mb as MB206;
                    if (mb206.sub_acno.Trim().Length > 0)
                        rptBox.AppendText(string.Format("\r\n{0}-{1} 成交回報回補 {2}", mb206.branch_id, mb206.sub_acno, mb206.toLog()));
                    else
                        rptBox.AppendText(string.Format("\r\n{0}-{1} 成交回報回補 {2}", mb206.branch_id, mb206.acno, mb206.toLog()));
                    rptBox.AppendText(Environment.NewLine);
                    break;
                //case 301:
                case 328:
                    MB328 mb328 = mb as MB328;
                    resultBox.AppendText(string.Format(" 權益數查詢成功:{0}", mb328.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 303:
                    MB303 mb303 = mb as MB303;
                    resultBox.AppendText(string.Format(" 客戶部位明細查詢成功:{0}", mb303.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 305:
                    MB305 mb305 = mb as MB305;
                    resultBox.AppendText(string.Format(" 客戶平倉明細查詢成功:{0}", mb305.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 308:
                    MB308 mb308 = mb as MB308;
                    resultBox.AppendText(string.Format(" 客戶平倉彙總查詢成功:{0}", mb308.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 310:
                    MB310 mb310 = mb as MB310;
                    resultBox.AppendText(string.Format(" 客戶部位彙總查詢成功:{0}", mb310.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 312:
                    MB312 mb312 = mb as MB312;
                    resultBox.AppendText(string.Format(" 客戶部位明細查詢成功:{0}", mb312.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 229:
                    {
                        MB229 mb229 = mb as MB229;
                        resultBox.AppendText(string.Format(" 客戶平倉明細MB229查詢成功:{0}", mb229.toLog()));
                        resultBox.AppendText(Environment.NewLine);
                        break;
                    }
                case 314:
                case 326:
                    MB314 mb314 = mb as MB314;
                    resultBox.AppendText(string.Format(" 客戶平倉明細彙總查詢成功:{0}", mb314.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 316:
                    MB316 mb316 = mb as MB316;
                    resultBox.AppendText(string.Format(" 客戶部位彙總查詢成功:{0}", mb316.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 320:
                case 323:
                    MB320 mb320 = mb as MB320;
                    resultBox.AppendText(string.Format(" VIP權益數查詢成功:{0}", mb320.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 322:
                case 324:
                    MB322 mb322 = mb as MB322;
                    resultBox.AppendText(string.Format(" VIP部位彙總查詢成功:{0}", mb322.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 501:
                case 505:
                    {
                        MB501 mb501 = mb as MB501;
                        int errorCode = 0;
                        if (int.TryParse(mb501.ErrCode, out errorCode) && (errorCode == 0))
                            rptBox.AppendText(string.Format("{0} 委託成功 {1}", mb501.UserDef, mb501.toLog()));
                        else
                            rptBox.AppendText(string.Format("{0} 委託失敗, ErrCode={1} ErrMsg={2}", mb501.UserDef, mb501.ErrCode, mb501.Msg));
                        rptBox.AppendText(Environment.NewLine);
                    }
                    break;
                case 502:
                case 506:
                    MB502 mb502 = mb as MB502;
                    rptBox.AppendText(string.Format("成交{0}口 {1}", mb502.DealQty, mb502.toLog()));
                    rptBox.AppendText(Environment.NewLine);
                    break;
                case 504:
                    MB504 mb504 = mb as MB504;
                    resultBox.AppendText(string.Format(" 外期回補回報狀態通知:{0}", mb504.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 511:
                    MB511 mb511 = mb as MB511;
                    resultBox.AppendText(string.Format(" 外期權益數查詢成功(大量):{0}", mb511.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 513:
                    MB513 mb513 = mb as MB513;
                    resultBox.AppendText(string.Format(" 外期部位彙總查詢成功(大量):{0}", mb513.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 515:
                    MB515 mb515 = mb as MB515;
                    resultBox.AppendText(string.Format(" 外期部位明細查詢成功(大量):{0}", mb515.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 517:
                    MB517 mb517 = mb as MB517;
                    resultBox.AppendText(string.Format(" 外期平倉明細查詢成功(大量):{0}", mb517.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                case 9000:
                    resultBox.AppendText(string.Format(" Note for market status:{0}", mb.toLog()));
                    resultBox.AppendText(Environment.NewLine);
                    break;
                default:
                    break;
            }
        }

        private void OnTAPIStatus(object sender, MESSAGE_TYPE status, string msg)
        {
            string ttxt = "";
            if (msg != null) ttxt = msg;
            switch (status)
            {
                case MESSAGE_TYPE.MT_CONNECT_READY:
                    logBox.AppendText(String.Format("{0}", ttxt));
                    logBox.AppendText(Environment.NewLine);
                    setButtons(3);
                    break;
                case MESSAGE_TYPE.MT_CONNECT_FAIL:
                    logBox.AppendText(String.Format("{0}", ttxt));
                    logBox.AppendText(Environment.NewLine);
                    setButtons(1);
                    break;
                case MESSAGE_TYPE.MT_DISCONNECTED:
                    {
                        if (InvokeRequired)//在非當前執行緒內 使用委派
                        {
                            Invoke(new Action<object, MESSAGE_TYPE, string>(OnTAPIStatus), sender, status, msg);
                        }
                        else
                        {
                            logBox.AppendText(String.Format("{0}", ttxt));
                            logBox.AppendText(Environment.NewLine);
                            setButtons(1);
                        }
                    }
                    break;
                case MESSAGE_TYPE.MT_SUBSCRIBE:
                    logBox.AppendText(String.Format("訂閱:{0}", ttxt));
                    logBox.AppendText(Environment.NewLine);
                    break;
                case MESSAGE_TYPE.MT_UNSUBSCRIBE:
                    logBox.AppendText(String.Format("解除訂閱!:{0}", ttxt));
                    logBox.AppendText(Environment.NewLine);
                    break;
                case MESSAGE_TYPE.MT_HEART_BEAT:
                    logBox.AppendText(String.Format("HeartBeat!:{0}", ttxt));
                    logBox.AppendText(Environment.NewLine);
                    break;
                case MESSAGE_TYPE.MT_LOGIN_OK:
                    logBox.AppendText(String.Format("{0}", ttxt));
                    logBox.AppendText(Environment.NewLine);
                    setButtons(4);
                    break;
                case MESSAGE_TYPE.MT_LOGIN_FAIL:
                    logBox.AppendText(String.Format("{0}", ttxt));
                    logBox.AppendText(Environment.NewLine);
                    setButtons(5);
                    break;
                case MESSAGE_TYPE.MT_ERROR:
                    logBox.AppendText(String.Format("其他錯誤:{0}", ttxt));
                    logBox.AppendText(Environment.NewLine);
                    break;
                case MESSAGE_TYPE.MT_RETRIEVE_FUT_DONE:
                    logBox.AppendText(String.Format("{0}", ttxt));
                    logBox.AppendText(Environment.NewLine);
                    break;
                case MESSAGE_TYPE.MT_RETRIEVE_OPT_DONE:
                    logBox.AppendText(String.Format("{0}", ttxt));
                    logBox.AppendText(Environment.NewLine);
                    break;
                case MESSAGE_TYPE.MT_RETRIEVE_STK_DONE:
                    logBox.AppendText(String.Format("{0}", ttxt));
                    logBox.AppendText(Environment.NewLine);
                    break;
                default:
                    if ((ttxt != null) && (!ttxt.Trim().Equals("")))
                    {
                        //logBox.AppendText(String.Format("{0}", ttxt));
                        //logBox.AppendText(Environment.NewLine);
                    }
                    break;
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                setButtons(2);
                tradeAPI.Disconnect();
            }
        }

        private void TradeAPIExampleForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (tradeAPI != null)
            {
                tradeAPI.Destroy();
                tradeAPI = null;
            }

        }

        private void btnWinLoginQ_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tradeAPI.AutoRetrieveProductInfo = chkAutoLoadProduct.Checked;
                tradeAPI.AutoSubReport = chkSubReports.Checked;
                tradeAPI.AutoRecoverReport = chkRecoverReports.Checked;
                tradeAPI.ShowLogin();
            }
        }

        private void chkSubReports_CheckedChanged(object sender, EventArgs e)
        {
            if (tradeAPI != null)
                tradeAPI.AutoSubReport = chkSubReports.Checked;
        }

        private void chkRecoverReports_CheckedChanged(object sender, EventArgs e)
        {
            if (tradeAPI != null)
                tradeAPI.AutoRecoverReport = chkRecoverReports.Checked;
        }

        private void btnRetriveProducts_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                int res = 0;
                res = tradeAPI.RetrieveProducts(cbxProductURL.Text, ckbForceToDownload.Checked);
                if (res < 0)
                {
                    tabCtlShow.SelectedTab = msgTab;
                    logBox.AppendText(String.Format("商品資料載入錯誤:{0};\r\n", tradeAPI.GetAPIAlertMsg(res)));
                }
            }
        }

        private void btnLoadLocalProducts_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                Tuple<int, string> res = tradeAPI.LoadAllLocalProducts();

                logBox.AppendText(String.Format("{0}\r\n", res));
            }
        }

        private void btnProList_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tabCtlShow.SelectedTab = queryResultTab;
                List<string> all_kinds = tradeAPI.GetProductBaseList();
                if (all_kinds == null) return;
                for (int i = 0; i < all_kinds.Count; i++)
                {
                    resultBox.AppendText(all_kinds[i]);
                    resultBox.AppendText(Environment.NewLine);
                }
            }
        }

        private void btnHot_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tabCtlShow.SelectedTab = queryResultTab;
                List<string> all_prods = null;
                switch (cbMarketType.SelectedIndex)
                {
                    case 0:
                        all_prods = tradeAPI.GetHotProducts();
                        // or all_prods = quoteAPI.GetHotProdicts(MARKET.FUTURES);
                        break;
                    case 1:
                        all_prods = tradeAPI.GetHotProducts(MARKET.FUTURES);
                        break;
                    case 2:
                        all_prods = tradeAPI.GetHotProducts(MARKET.OPTION);
                        break;
                }
                if (all_prods == null) return;
                for (int i = 0; i < all_prods.Count; i++)
                {
                    resultBox.AppendText(all_prods[i]);
                    resultBox.AppendText(Environment.NewLine);
                }
            }
        }

        private void btnPoint_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                int dec = tradeAPI.GetStrikePriceDecimal(txtComID.Text.Trim());
                if (dec >= 0)
                    resultBox.AppendText("履約價小數位數：" + Convert.ToString(dec));
                else
                    resultBox.AppendText("履約價小數位數：找不到商品");
                resultBox.AppendText(Environment.NewLine);
            }
        }

        private void btnGet1801_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tabCtlShow.SelectedTab = queryResultTab;
                PT01801 data = tradeAPI.GetProductBase(txtKindID.Text.Trim());
                if (data == null) return;
                resultBox.AppendText(data.ComId);
                resultBox.AppendText("|");
                resultBox.AppendText(data.ComCName);
                resultBox.AppendText("|");
                resultBox.AppendText(Convert.ToString(data.ComType));
                resultBox.AppendText("|");
                resultBox.AppendText(Convert.ToString(data.ContractType));
                resultBox.AppendText("|");
                resultBox.AppendText(Convert.ToString(data.ContractValue));
                resultBox.AppendText("|");
                resultBox.AppendText(Convert.ToString(data.StkPriceDecimal));
                resultBox.AppendText(Environment.NewLine);
            }
        }

        private void btnProcInfo_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tabCtlShow.SelectedTab = queryResultTab;
                PT01802 data = tradeAPI.GetProductInfo(txtComID.Text.Trim());
                if (data == null) return;
                resultBox.AppendText(data.ComId);
                resultBox.AppendText("|");
                resultBox.AppendText(Convert.ToString(data.FallPrice));
                resultBox.AppendText("|");
                resultBox.AppendText(Convert.ToString(data.RisePrice));
                resultBox.AppendText("|");
                resultBox.AppendText(Convert.ToString(data.Hot));
                resultBox.AppendText("|");
                resultBox.AppendText(Convert.ToString(data.StkPriceDecimal));
                resultBox.AppendText(Environment.NewLine);
            }
        }

        private void btnDList_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tabCtlShow.SelectedTab = queryResultTab;
                List<PT01802> allKindDetail = tradeAPI.GetProductDetailList(txtKindID.Text);
                if (allKindDetail == null) return;
                for (int i = 0; i < allKindDetail.Count; i++)
                {
                    resultBox.AppendText(allKindDetail[i].ComId);
                    resultBox.AppendText(Environment.NewLine);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tabCtlShow.SelectedTab = queryResultTab;
                List<string> all_industGroups = tradeAPI.GetIndustryGroups();
                if (all_industGroups == null) return;
                for (int i = 0; i < all_industGroups.Count; i++)
                {
                    resultBox.AppendText(all_industGroups[i]);
                    resultBox.AppendText(Environment.NewLine);
                }
            }
        }

        private void btnskind_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tabCtlShow.SelectedTab = queryResultTab;
                string qstring = txtQProList.Text.Trim();
                STOCK_QUERY_FIELD field;
                field = (STOCK_QUERY_FIELD)cbProListField.SelectedIndex;
                List<PIProList> all_kinds = tradeAPI.GetProListByStockKind(field, qstring);
                if (all_kinds != null)
                {
                    for (int i = 0; i < all_kinds.Count; i++)
                    {
                        resultBox.AppendText(all_kinds[i].StockCode);
                        resultBox.AppendText("|");
                        resultBox.AppendText(all_kinds[i].StockNo);
                        resultBox.AppendText("|");
                        resultBox.AppendText(all_kinds[i].StockName);
                        resultBox.AppendText("|");
                        resultBox.AppendText(all_kinds[i].StockKind);
                        resultBox.AppendText(Environment.NewLine);
                    }
                }
                all_kinds = null;
            }
        }

        private void btnRetrieveCover_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tradeAPI.RetrieveCover(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text);
            }
        }

        private void btnRetrieveCoverDetail_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                if (ckbCoverDetailMass.Checked)
                {
                    if (txtCoverType.Text == "1")
                        tradeAPI.RetrieveCoverDetailEx(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text, '1');
                    else if (txtCoverType.Text == "2")
                        tradeAPI.RetrieveCoverDetailEx(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text, '2');
                    else
                        tradeAPI.RetrieveCoverDetailEx(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text);
                }
                else
                    tradeAPI.RetrieveCoverDetail(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text);
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tradeAPI.RetrieveCoverDetailHistoryEx(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text, int.Parse(textBox3.Text), int.Parse(textBox4.Text));
            }
        }

        private void btnRetrieveFMargin_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tradeAPI.RetrieveFMargin(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text);
            }
        }

        private void btnRetrievePosition_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                if (ckbPositionSummaryMass.Checked)
                    tradeAPI.RetrievePositionSumEx(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text);
                else
                    tradeAPI.RetrievePositionSum(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text);
            }
        }

        private void btnRetrievePositionDetail_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                if (ckbPositionDetailMass.Checked)
                    tradeAPI.RetrievePositionDetailEx(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text);
                else
                    tradeAPI.RetrievePositionDetail(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text);
            }
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            FUNCTION func = FUNCTION.NEW;
            switch (cbFunc.SelectedIndex)
            {
                case 0:
                    func = FUNCTION.NEW;
                    break;
                case 1:
                    func = FUNCTION.CH_QTY;
                    break;
                case 2:
                    func = FUNCTION.CH_PRICE;
                    break;
                case 3:
                    func = FUNCTION.CANCEL;
                    break;
            }

            MARKET market = MARKET.FUTURES;
            switch (cbMarket.SelectedIndex)
            {
                case 0:
                    market = MARKET.FUTURES;
                    break;
                case 1:
                    market = MARKET.OPTION;
                    break;
            }

            SIDE side = SIDE.BUY;
            switch (cbSide.SelectedIndex)
            {
                case 0:
                    side = SIDE.BUY;
                    break;
                case 1:
                    side = SIDE.SELL;
                    break;
            }

            PRICEFLAG pf = PRICEFLAG.LIMIT_PRICE;
            if (rbSP.Checked)
                pf = PRICEFLAG.LIMIT_PRICE;
            else if (rbMP.Checked)
                pf = PRICEFLAG.MARKET_PRICE;
            else if (rbPP.Checked)
                pf = PRICEFLAG.R_PRICE;

            decimal price = 0;
            if (tbTPrice.Text.Trim().Length > 0)
                price = Convert.ToDecimal(tbTPrice.Text);

            TIME_IN_FORCE tif = TIME_IN_FORCE.ROD;
            switch (cbTif.SelectedIndex)
            {
                case 0:
                    tif = TIME_IN_FORCE.ROD;
                    break;
                case 1:
                    tif = TIME_IN_FORCE.IOC;
                    break;
                case 2:
                    tif = TIME_IN_FORCE.FOK;
                    break;
            }

            ushort qty = 0;
            if (tbTQty.Text.Trim().Length > 0)
                qty = Convert.ToUInt16(tbTQty.Text.Trim());

            POSITION_EFFECT pe = POSITION_EFFECT.OPEN;
            switch (cbPositionFlag.SelectedIndex)
            {
                case 0:
                    pe = POSITION_EFFECT.OPEN;
                    break;
                case 1:
                    pe = POSITION_EFFECT.CLOSE;
                    break;
                case 2:
                    pe = POSITION_EFFECT.HEDGE;
                    break;
                case 3:
                    pe = POSITION_EFFECT.AUTO;
                    break;
            }

            ORDER_RETURN_CODE rc = tradeAPI.Order(func, market, tbUserDefineId.Text.Trim(), tbTBrokerID.Text.Trim(), tbTAccount.Text.Trim(), tbTSubAccount.Text.Trim(), tbTSymbol.Text.Trim(),
                           tbOrigSeqNo.Text.Trim(), side, pf, price, tif, qty, pe, tbOrderNo.Text.Trim());
            string msg;
            switch (rc)
            {
                case ORDER_RETURN_CODE.SUCCESS:
                    msg = "委託傳送成功";
                    break;
                case ORDER_RETURN_CODE.CERT_NOT_FOUND:
                    msg = "憑證錯誤";
                    break;
                case ORDER_RETURN_CODE.SIGN_OBJECT_ERROR:
                    msg = "簽章元件錯誤";
                    break;
                case ORDER_RETURN_CODE.ACCOUNT_NOT_FOUND_ERROR:
                    msg = "無可下單帳號";
                    break;
                case ORDER_RETURN_CODE.PRICE_ZERO_ERROR:
                    msg = "限價單價格不得為0";
                    break;
                case ORDER_RETURN_CODE.SYMBOL_LENGTH_ERROR:
                    msg = "商品代碼長度錯誤";
                    break;
                case ORDER_RETURN_CODE.ORDERNO_EMPTY_ERROR:
                    msg = "刪改單，請輸入委託書號";
                    break;
                //case ORDER_RETURN_CODE.NO_SPEEDY_AUTHORIZE:
                //    msg = "本帳號無Speedy下單權限";
                //    break;
                default:
                    msg = "其他錯誤";
                    break;
            }
            logBox.AppendText(String.Format("{0}", msg));
            logBox.AppendText(Environment.NewLine);
        }

        private void btnDestroy_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                if (tradeAPI.ConnectionStatus == CONNECTION_STATUS.CS_CONNECTREADY)
                {
                    tradeAPI.Disconnect();
                }
                tradeAPI.Destroy();
                tradeAPI = null;
            }
        }

        private void btnAccountDetail_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                if (tradeAPI.ConnectionStatus == CONNECTION_STATUS.CS_CONNECTREADY)
                {
                    int iCnt = tradeAPI.AccountCount();
                    for (int i = 0; i < iCnt; i++)
                    {
                        Account acc = tradeAPI.AccountDetail(i);
                        logBox.AppendText(String.Format("id = {0}", acc.account_id));
                        logBox.AppendText(Environment.NewLine);
                        logBox.AppendText(String.Format("type = {0}", acc.account_type));
                        logBox.AppendText(Environment.NewLine);
                        logBox.AppendText(String.Format("branch = {0}", acc.branch_code));
                        logBox.AppendText(Environment.NewLine);
                        logBox.AppendText(String.Format("comp = {0}", acc.comp_code));
                        logBox.AppendText(Environment.NewLine);
                        logBox.AppendText(String.Format("customer = {0}", acc.customer_type));
                        logBox.AppendText(Environment.NewLine);
                        logBox.AppendText(String.Format("subAccount = {0}", acc.subaccount_id));
                        logBox.AppendText(Environment.NewLine);
                        logBox.AppendText(String.Format("dayTrade = {0}", acc.day_trade));
                        logBox.AppendText(Environment.NewLine);
                        logBox.AppendText(String.Format("do_cert = {0}", acc.do_cert));
                        logBox.AppendText(Environment.NewLine);
                        logBox.AppendText(String.Format("interOrder = {0}", acc.internal_order));
                        logBox.AppendText(Environment.NewLine);
                        logBox.AppendText(String.Format("foreign = {0}", acc.is_foreign));
                        logBox.AppendText(Environment.NewLine);
                        logBox.AppendText(String.Format("MIT = {0}", acc.is_MIT));
                        logBox.AppendText(Environment.NewLine);
                        logBox.AppendText(String.Format("WLT = {0}", acc.is_WLT));
                        logBox.AppendText(Environment.NewLine);
                        logBox.AppendText(String.Format("actKind = {0}", acc.actKind));
                        logBox.AppendText(Environment.NewLine);
                        logBox.AppendText(String.Format("speedy = {0}", acc.is_speed_act));
                        logBox.AppendText(Environment.NewLine);
                        logBox.AppendText(String.Format("sales = {0}", acc.sales_code));
                        logBox.AppendText(Environment.NewLine);
                    }
                }
            }
        }

        private void btnSubReport_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < tradeAPI.AccountCount(); i++)
            {
                Account acc = tradeAPI.AccountDetail(i);
                if (acc.subaccount_id.Trim().Equals(""))
                    tradeAPI.SubAccount(acc.branch_code.Trim() + acc.account_id);
                else
                    tradeAPI.SubAccount(acc.subaccount_id);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tradeAPI.RecoverReport(RECOVER_TYPE.COVER_TYPE_ALL);
        }

        private void btnTradeLogin_Click(object sender, EventArgs e)
        {
            string account = tbID.Text.Trim();
            string subaccount = tbSubAccount.Text.Trim();
            tradeAPI.AutoRetrieveProductInfo = chkAutoLoadProduct.Checked;
            tradeAPI.AutoSubReport = chkSubReports.Checked;
            tradeAPI.AutoRecoverReport = chkRecoverReports.Checked;
            if (chkAutoLoadProduct.Checked)
                tradeAPI.SetProductFileBaseURL(cbxProductURL.Text);
            tradeAPI.LoginTrade(account, tbPwd.Text, subaccount);
        }

        private void btnTseProds_Click(object sender, EventArgs e)
        {
            tabCtlShow.SelectedTab = queryResultTab;
            switch (cbStkQuery1.SelectedIndex)
            {
                case 1:
                    List<string> ll = tradeAPI.GetStockkGroupList(STOCK_GROUP.OTC, (bool)ckbStkQryWithName.Checked);
                    if (ll != null)
                    {
                        foreach (string ss in ll)
                        {
                            resultBox.AppendText(ss);
                            resultBox.AppendText(Environment.NewLine);
                        }
                    }
                    break;
                case 2:
                    List<PRODUCT_WARRANT_DETAIL> ll_2 = tradeAPI.GetStockWarrantList(txtStockId1.Text);
                    if (ll_2 != null)
                    {
                        foreach (PRODUCT_WARRANT_DETAIL item in ll_2)
                        {
                            if ((bool)ckbStkQryWithName.Checked)
                                resultBox.AppendText(item.warrant_id + "," + item.warrant_name);
                            else
                                resultBox.AppendText(item.warrant_id);
                            resultBox.AppendText(Environment.NewLine);
                        }
                    }
                    break;
                default:
                    List<string> ll_3 = tradeAPI.GetStockkGroupList(STOCK_GROUP.TSE, (bool)ckbStkQryWithName.Checked);
                    if (ll_3 != null)
                    {
                        foreach (string ss in ll_3)
                            resultBox.AppendText(ss);
                        resultBox.AppendText(Environment.NewLine);
                    }
                    break;
            }
        }

        private void btnProductDetail_Click(object sender, EventArgs e)
        {
            tabCtlShow.SelectedTab = queryResultTab;
            switch (cbStkQuery2.SelectedIndex)
            {
                case 1:
                    PRODUCT_WARRANT_DETAIL wrn = tradeAPI.WarrantProductDetail(txtStockId2.Text);
                    if (wrn != null)
                    {
                        if (wrn.group == STOCK_GROUP.TSE)
                            resultBox.AppendText("市場：上市");
                        else
                            resultBox.AppendText("市場：上櫃");
                        resultBox.AppendText(Environment.NewLine);

                        resultBox.AppendText("商品代碼：" + wrn.warrant_id);
                        resultBox.AppendText(Environment.NewLine);
                        resultBox.AppendText("商品名：" + wrn.warrant_name);
                        resultBox.AppendText(Environment.NewLine);
                        resultBox.AppendText("產業：" + wrn.target_id);
                        resultBox.AppendText(Environment.NewLine);
                        resultBox.AppendText("Up：" + wrn.up_limit_price);
                        resultBox.AppendText(Environment.NewLine);
                        resultBox.AppendText("exec：" + wrn.strike_price);
                        resultBox.AppendText(Environment.NewLine);
                        resultBox.AppendText("Down：" + wrn.down_limit_price);
                        resultBox.AppendText(Environment.NewLine);
                    }
                    break;
                default:
                    PRODUCT_STOCK_DETAIL stk = tradeAPI.StockProductDetail(txtStockId2.Text);
                    if (stk != null)
                    {
                        if (stk.group == STOCK_GROUP.TSE)
                            resultBox.AppendText("市場：上市");
                        else
                            resultBox.AppendText("市場：上櫃");
                        resultBox.AppendText(Environment.NewLine);

                        resultBox.AppendText("商品代碼：" + stk.prod_id);
                        resultBox.AppendText(Environment.NewLine);
                        resultBox.AppendText("商品名：" + stk.prod_name);
                        resultBox.AppendText(Environment.NewLine);
                        resultBox.AppendText("產業：" + stk.industry_id);
                        resultBox.AppendText(Environment.NewLine);
                        resultBox.AppendText("漲停：" + stk.up_limit_price);
                        resultBox.AppendText(Environment.NewLine);
                        resultBox.AppendText("參考：" + stk.reference_price);
                        resultBox.AppendText(Environment.NewLine);
                        resultBox.AppendText("跌停：" + stk.down_limit_price);
                        resultBox.AppendText(Environment.NewLine);
                    }
                    break;
            }
        }

        private void btnTseOtcIndex_Click(object sender, EventArgs e)
        {
            List<string> ll_1 = tradeAPI.GetStockIndeies(STOCK_GROUP.TSE);
            if (ll_1 != null)
                foreach (string ss in ll_1)
                {
                    resultBox.AppendText(ss);
                    resultBox.AppendText(Environment.NewLine);
                }

            resultBox.AppendText("======================");
            resultBox.AppendText(Environment.NewLine);

            List<string> ll_2 = tradeAPI.GetStockIndeies(STOCK_GROUP.OTC);
            if (ll_2 != null)
                foreach (string ss in ll_2)
                {
                    resultBox.AppendText(ss);
                    resultBox.AppendText(Environment.NewLine);
                }

        }

        private void btnTseOtcNewTaiwanIndex_Click(object sender, EventArgs e)
        {
            List<string> ll_1 = tradeAPI.GetStockTwIndeies(STOCK_GROUP.TSE);
            if (ll_1 != null)
                foreach (string ss in ll_1)
                {
                    resultBox.AppendText(ss);
                    resultBox.AppendText(Environment.NewLine);
                }

            resultBox.AppendText("======================");
            resultBox.AppendText(Environment.NewLine);

            List<string> ll_2 = tradeAPI.GetStockTwIndeies(STOCK_GROUP.OTC);
            if (ll_2 != null)
                foreach (string ss in ll_2)
                {
                    resultBox.AppendText(ss);
                    resultBox.AppendText(Environment.NewLine);
                }
        }

        private void btnOrderStk_Click(object sender, EventArgs e)
        {
            FUNCTION_STOCK func = FUNCTION_STOCK.NEW;
            switch (cbSFunc.SelectedIndex)
            {
                case 0:
                    func = FUNCTION_STOCK.NEW;
                    break;
                case 1:
                    func = FUNCTION_STOCK.CH_QTY;
                    break;
                case 2:
                    func = FUNCTION_STOCK.CANCEL;
                    break;
            }

            STOCK_ORDER_GROUP market = STOCK_ORDER_GROUP.TSE;
            switch (cbSMarket.SelectedIndex)
            {
                case 0:
                    market = STOCK_ORDER_GROUP.TSE;
                    break;
                case 1:
                    market = STOCK_ORDER_GROUP.OTC;
                    break;
            }

            SIDE side = SIDE.BUY;
            switch (cbSBS.SelectedIndex)
            {
                case 0:
                    side = SIDE.BUY;
                    break;
                case 1:
                    side = SIDE.SELL;
                    break;
                default:
                    return;
            }

            PRICEFLAG_STOCK pf = PRICEFLAG_STOCK.LIMIT_PRICE;
            switch (cbSPriceType.SelectedIndex)
            {
                case 0:
                    pf = PRICEFLAG_STOCK.LIMIT_PRICE;
                    break;
                case 1:
                    pf = PRICEFLAG_STOCK.UP_LIMIT_PRICE;
                    break;
                case 2:
                    pf = PRICEFLAG_STOCK.DOWN_LIMIT_PRICE;
                    break;
                case 3:
                    pf = PRICEFLAG_STOCK.MARKET_PRICE; ;
                    break;
                case 4:
                    pf = PRICEFLAG_STOCK.REFERENCE_PRICE;
                    break;
            }

            decimal price = 0;
            if (tbSPrcie.Text.Trim().Length > 0)
                price = Convert.ToDecimal(tbSPrcie.Text);


            STOCK_TRADE_SEGMENT ts = STOCK_TRADE_SEGMENT.NORMAL;
            switch (cbSTradeSection.SelectedIndex)
            {
                case 0:
                    ts = STOCK_TRADE_SEGMENT.NORMAL;
                    break;
                case 1:
                    ts = STOCK_TRADE_SEGMENT.AFTER_MARKET;
                    break;
                case 2:
                    ts = STOCK_TRADE_SEGMENT.ODD_LOT;
                    break;
            }

            ushort qty = 0;
            if (tbSQty.Text.Trim().Length > 0)
                qty = Convert.ToUInt16(tbSQty.Text.Trim());

            STOCK_ORDER_TYPE ot = STOCK_ORDER_TYPE.NORMAL;
            switch (cbSOrderType.SelectedIndex)
            {
                case 0:
                    ot = STOCK_ORDER_TYPE.NORMAL;
                    break;
                case 1:
                    ot = STOCK_ORDER_TYPE.MARGIN_PURCHASE;
                    break;
                case 2:
                    ot = STOCK_ORDER_TYPE.SHORT_SALE;
                    break;
                case 3:
                    ot = STOCK_ORDER_TYPE.NORMAL_STOCK_LENDING_SALE;
                    break;
                case 4:
                    ot = STOCK_ORDER_TYPE.SHORT_LENDING_SALE;
                    break;
                case 5:
                    ot = STOCK_ORDER_TYPE.OFFSET_SALE;
                    break;
                default:
                    return;
            }

            ORDER_RETURN_CODE rc = tradeAPI.Order_Stock(func, market, tbSUserDefine.Text.Trim(), tbSBrokerID.Text.Trim(), tbSAccount.Text.Trim(), tbSSubAccount.Text.Trim(), tbSSymbol.Text.Trim(),
                           tbSSalesId.Text.Trim(), side, pf, price, ts, qty, ot, tbSOrderNo.Text.Trim());
            string msg;
            switch (rc)
            {
                case ORDER_RETURN_CODE.SUCCESS:
                    msg = "委託傳送成功";
                    break;
                case ORDER_RETURN_CODE.CERT_NOT_FOUND:
                    msg = "憑證錯誤";
                    break;
                case ORDER_RETURN_CODE.SIGN_OBJECT_ERROR:
                    msg = "簽章元件錯誤";
                    break;
                case ORDER_RETURN_CODE.ACCOUNT_NOT_FOUND_ERROR:
                    msg = "無可下單帳號";
                    break;
                case ORDER_RETURN_CODE.PRICE_ZERO_ERROR:
                    msg = "限價單價格不得為0";
                    break;
                case ORDER_RETURN_CODE.SYMBOL_LENGTH_ERROR:
                    msg = "商品代碼長度錯誤";
                    break;
                case ORDER_RETURN_CODE.ORDERNO_EMPTY_ERROR:
                    msg = "刪改單，請輸入委託書號";
                    break;
                case ORDER_RETURN_CODE.NO_SPEEDY_AUTHORIZE:
                    msg = "本帳號無Speedy下單權限";
                    break;
                default:
                    msg = "其他錯誤";
                    break;
            }
            logBox.AppendText(String.Format("{0}", msg));
            logBox.AppendText(Environment.NewLine);
        }

        private void buttonRetrieveMB410_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tradeAPI.RetrievePositionStock(tbSBrokerID.Text, tbSAccount.Text, tbSSubAccount.Text, tbSUserDefine.Text);
            }
        }

        private void buttonRetrieveMB412_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tradeAPI.RetrieveBalanceStock(tbSBrokerID.Text, tbSAccount.Text, tbSSubAccount.Text, tbSUserDefine.Text);
            }
        }

        private void buttonRetrieveMB414_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tradeAPI.RetrieveAccountCmrStock(tbSBrokerID.Text, tbSAccount.Text, tbSSubAccount.Text, tbSUserDefine.Text);
            }
        }

        private void btnNoticeQuery_Click(object sender, EventArgs e)
        {
            if ((tradeAPI == null) || (!tradeAPI.LoginOK))
                MessageBox.Show("請先登入!!");
            else
                tradeAPI.RetrieveNotice(tbNoticeFromDate.Text, tbNoticeToDate.Text);
        }

        private void btnOrderFFutures_Click(object sender, EventArgs e)
        {
            if (tradeAPI == null)
            {
                return;
            }
            //else if (string.IsNullOrWhiteSpace(tbxFFutures2.Text) || (tbxFFutures2.Text.Length < 10))
            //{
            //    logBox.AppendText("[T](Order)查無帳號");
            //    logBox.AppendText(Environment.NewLine);
            //    return;
            //}

            MB500 mb500 = new MB500()
            {
                fcode = tbxFFutures1.Text[0],
                branch_id = tbxFFutures2.Text.Trim(),
                acno = textBox5.Text.Trim(),
                Department = textBox6.Text.Trim(),
                sub_acno = textBox7.Text.Trim(),
                exchange = tbxFFutures3.Text,
                seq_no = tbxFFutures4.Text,
                bs = (string.IsNullOrEmpty(tbxFFutures5.Text) ? ' ' : tbxFFutures5.Text[0]),
                commodity = tbxFFutures6.Text,
                com_ym = (string.IsNullOrEmpty(tbxFFutures7.Text) ? DateTime.Now.ToString("yyyyMM") : tbxFFutures7.Text),
                //com_ym2 = "",
                strike_pri = tbxFFutures8.Text,
                cp = (string.IsNullOrEmpty(tbxFFutures9.Text) ? ' ' : tbxFFutures9.Text[0]),
                qty = tbxFFutures10.Text,
                price = tbxFFutures11.Text,
                stop_pri = tbxFFutures12.Text,
                pri_type = (string.IsNullOrEmpty(tbxFFutures13.Text) ? ' ' : tbxFFutures13.Text[0]),
                time_in_force = (string.IsNullOrEmpty(tbxFFutures14.Text) ? ' ' : tbxFFutures14.Text[0]),
                open_close = (string.IsNullOrEmpty(tbxFFutures15.Text) ? ' ' : tbxFFutures15.Text[0]),
                day_trade = (string.IsNullOrEmpty(tbxFFutures16.Text) ? ' ' : tbxFFutures16.Text[0]),
                //order_date = (string.IsNullOrEmpty(tbxFFutures17.Text) ? DateTime.Now.ToString("yyyyMMdd") : tbxFFutures17.Text),
                //order_time = (string.IsNullOrEmpty(tbxFFutures18.Text) ? DateTime.Now.ToString("HHmmssfff") : tbxFFutures18.Text),
                order_no = tbxFFutures19.Text,
                //temp = "",
                user_def = tbxFFutures20.Text,
            };

            ORDER_RETURN_CODE rc = tradeAPI.OrderFFutures(mb500, (mb500.branch_id + mb500.acno + mb500.sub_acno), mb500.Department);
            string msg;
            switch (rc)
            {
                case ORDER_RETURN_CODE.SUCCESS:
                    msg = "委託傳送成功";
                    break;
                case ORDER_RETURN_CODE.CERT_NOT_FOUND:
                    msg = "憑證錯誤";
                    break;
                case ORDER_RETURN_CODE.SIGN_OBJECT_ERROR:
                    msg = "簽章元件錯誤";
                    break;
                case ORDER_RETURN_CODE.ACCOUNT_NOT_FOUND_ERROR:
                    msg = "無可下單帳號";
                    break;
                //case ORDER_RETURN_CODE.SYMBOL_LENGTH_ERROR:
                //    msg = "商品代碼長度錯誤";
                //    break;
                //case ORDER_RETURN_CODE.ORDERNO_EMPTY_ERROR:
                //    msg = "刪改單，請輸入委託書號";
                //    break;
                case ORDER_RETURN_CODE.NO_SPEEDY_AUTHORIZE:
                    msg = "本帳號無Speedy下單權限";
                    break;
                case ORDER_RETURN_CODE.ORDERNO_NOT_EMPTY_ERROR:
                    msg = "新單，請勿輸入委託書號";
                    break;
                default:
                    msg = "其他錯誤";
                    break;
            }
            logBox.AppendText(String.Format("{0}", msg));
            logBox.AppendText(Environment.NewLine);
        }

        private void btnRecoverReportForFF_Click(object sender, EventArgs e)
        {
            tradeAPI.RecoverReportForFF(RECOVER_TYPE.COVER_TYPE_ALL);
        }

        private void btnRetrieveFFMargin_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                string BrokerID = tbxFFutures2.Text.Trim();
                string Account = textBox5.Text.Trim();
                string SubAccount = textBox7.Text.Trim();

                tradeAPI.RetrieveFFMargin(BrokerID, Account, SubAccount, textBox6.Text.Trim(), tbxFFutures20.Text);
            }
        }

        private void btnRetrieveFFPositionSumEx_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                string BrokerID = tbxFFutures2.Text.Trim();
                string Account = textBox5.Text.Trim();
                string SubAccount = textBox7.Text.Trim();

                tradeAPI.RetrieveFFPositionSumEx(BrokerID, Account, SubAccount, tbxFFutures3.Text, textBox6.Text.Trim());
            }
        }

        private void btnRetrieveFFPositionDetailEx_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                string BrokerID = tbxFFutures2.Text.Trim();
                string Account = textBox5.Text.Trim();
                string SubAccount = textBox7.Text.Trim();

                tradeAPI.RetrieveFFPositionDetailEx(BrokerID, Account, SubAccount, tbxFFutures3.Text, textBox6.Text.Trim());
            }
        }

        private void btnRetrieveFFCoverDetailEx_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                string BrokerID = tbxFFutures2.Text.Trim();
                string Account = textBox5.Text.Trim();
                string SubAccount = textBox7.Text.Trim();

                //tradeAPI.RetrieveFFCoverDetailEx(BrokerID, Account, SubAccount, tbxFFutures3.Text, int.Parse(textBoxQueryStartDate.Text), int.Parse(textBoxQueryEndDate.Text), textBox6.Text.Trim());
                tradeAPI.RetrieveCoverDetailEx(BrokerID, Account, SubAccount, '1');
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (tradeAPI == null)
            {
                return;
            }

            tradeAPI.RetrieveFVIPMargin(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text, tbUserDefineId.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (tradeAPI == null)
            {
                return;
            }

            tradeAPI.RetrieveFVIPPositionSumEx(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tradeAPI.RetrieveMB416(tbSBrokerID.Text, tbSAccount.Text, tbSSubAccount.Text, tbSUserDefine.Text);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tradeAPI.RetrieveMB418(tbSBrokerID.Text, tbSAccount.Text, tbSSubAccount.Text, tbSUserDefine.Text);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tradeAPI.RetrieveMB420(tbSBrokerID.Text, tbSAccount.Text, tbSSubAccount.Text, int.Parse(textBox1.Text), int.Parse(textBox2.Text), tbSUserDefine.Text);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                char bs = '0';

                switch (cbSBS.SelectedIndex)
                {
                    case 0:
                        bs = '1';
                        break;
                    case 1:
                        bs = '2';
                        break;
                }

                tradeAPI.RetrieveMB422(tbSBrokerID.Text, tbSAccount.Text, tbSSubAccount.Text, int.Parse(textBox1.Text), int.Parse(textBox2.Text), tbSSymbol.Text.Trim(), bs, tbSUserDefine.Text);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                char orderType = 'A';

                switch (cbSOrderType.SelectedIndex)
                {
                    case 0:
                        orderType = '0';
                        break;
                    case 1:
                        orderType = '1';
                        break;
                    case 2:
                        orderType = '2';
                        break;
                    case 6:
                        orderType = '3';
                        break;
                    case 7:
                        orderType = '4';
                        break;
                }

                tradeAPI.RetrieveMB424(tbSBrokerID.Text, tbSAccount.Text, tbSSubAccount.Text, int.Parse(textBox1.Text), int.Parse(textBox2.Text), tbSSymbol.Text.Trim(), orderType, tbSUserDefine.Text);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                char orderType = 'A';

                switch (cbSOrderType.SelectedIndex)
                {
                    case 0:
                        orderType = '0';
                        break;
                    case 1:
                        orderType = '1';
                        break;
                    case 2:
                        orderType = '2';
                        break;
                    case 6:
                        orderType = '3';
                        break;
                    case 7:
                        orderType = '4';
                        break;
                }

                tradeAPI.RetrieveMB426(tbSBrokerID.Text, tbSAccount.Text, tbSSubAccount.Text, tbSSymbol.Text.Trim(), orderType, tbSUserDefine.Text);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                char orderType = 'A';

                switch (cbSOrderType.SelectedIndex)
                {
                    case 0:
                        orderType = '0';
                        break;
                    case 1:
                        orderType = '1';
                        break;
                    case 2:
                        orderType = '2';
                        break;
                    case 6:
                        orderType = '3';
                        break;
                    case 7:
                        orderType = '4';
                        break;
                }

                tradeAPI.RetrieveMB428(tbSBrokerID.Text, tbSAccount.Text, tbSSubAccount.Text, tbSSymbol.Text.Trim(), orderType, tbSUserDefine.Text);
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tradeAPI.RetrieveMB430(tbSBrokerID.Text, tbSAccount.Text, tbSSubAccount.Text, textBox1.Text, tbSOrderNo.Text, tbSSymbol.Text, decimal.Parse(tbSPrcie.Text), tbSUserDefine.Text);
            }
        }

        private void btnRetrievePositionDetailSingle_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                if (ckbPositionDetailMass.Checked)
                    tradeAPI.RetrievePositionDetailSingle(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text);
                else
                    tradeAPI.RetrievePositionDetail(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text);
            }
        }

        private void btnRetrieveOrder_Click(object sender, EventArgs e)
        {
            if (tradeAPI != null)
            {
                tradeAPI.RetrieveOrders(tbTBrokerID.Text, tbTAccount.Text, tbTSubAccount.Text, tbUserDefineId.Text, 'O');
            }
        }
        /// <summary>
        /// 外期價差下單
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button14_Click(object sender, EventArgs e)
        {
            if (tradeAPI == null)
            {
                return;
            }
            //else if (string.IsNullOrWhiteSpace(tbxFFutures2.Text) || (tbxFFutures2.Text.Length < 10))
            //{
            //    logBox.AppendText("[T](Order)查無帳號");
            //    logBox.AppendText(Environment.NewLine);
            //    return;
            //}

            MB500 mb500 = new MB500()
            {
                fcode = tbxFFutures1.Text[0],
                branch_id = tbxFFutures2.Text.Trim(),
                acno = textBox5.Text.Trim(),
                Department = textBox6.Text.Trim(),
                sub_acno = textBox7.Text.Trim(),
                exchange = tbxFFutures3.Text,
                seq_no = tbxFFutures4.Text,
                bs = (string.IsNullOrEmpty(tbxFFutures5.Text) ? ' ' : tbxFFutures5.Text[0]),
                commodity = tbxFFutures6.Text,
                com_ym = (string.IsNullOrEmpty(tbxFFutures7.Text) ? DateTime.Now.ToString("yyyyMM") : tbxFFutures7.Text),
                //com_ym2 = (string.IsNullOrEmpty(textBox8.Text) ? DateTime.Now.ToString("yyyyMM") : textBox8.Text),
                strike_pri = tbxFFutures8.Text,
                cp = (string.IsNullOrEmpty(tbxFFutures9.Text) ? ' ' : tbxFFutures9.Text[0]),
                qty = tbxFFutures10.Text,
                price = tbxFFutures11.Text,
                stop_pri = tbxFFutures12.Text,
                pri_type = (string.IsNullOrEmpty(tbxFFutures13.Text) ? ' ' : tbxFFutures13.Text[0]),
                time_in_force = (string.IsNullOrEmpty(tbxFFutures14.Text) ? ' ' : tbxFFutures14.Text[0]),
                open_close = (string.IsNullOrEmpty(tbxFFutures15.Text) ? ' ' : tbxFFutures15.Text[0]),
                day_trade = (string.IsNullOrEmpty(tbxFFutures16.Text) ? ' ' : tbxFFutures16.Text[0]),
                //order_date = (string.IsNullOrEmpty(tbxFFutures17.Text) ? DateTime.Now.ToString("yyyyMMdd") : tbxFFutures17.Text),
                //order_time = (string.IsNullOrEmpty(tbxFFutures18.Text) ? DateTime.Now.ToString("HHmmssfff") : tbxFFutures18.Text),
                order_no = tbxFFutures19.Text,
                //temp = "",
                user_def = tbxFFutures20.Text,
            };

            ORDER_RETURN_CODE rc = tradeAPI.OrderFFutures(mb500, (mb500.branch_id + mb500.acno + mb500.sub_acno), mb500.Department);
            string msg;
            switch (rc)
            {
                case ORDER_RETURN_CODE.SUCCESS:
                    msg = "委託傳送成功";
                    break;
                case ORDER_RETURN_CODE.CERT_NOT_FOUND:
                    msg = "憑證錯誤";
                    break;
                case ORDER_RETURN_CODE.SIGN_OBJECT_ERROR:
                    msg = "簽章元件錯誤";
                    break;
                case ORDER_RETURN_CODE.ACCOUNT_NOT_FOUND_ERROR:
                    msg = "無可下單帳號";
                    break;
                //case ORDER_RETURN_CODE.SYMBOL_LENGTH_ERROR:
                //    msg = "商品代碼長度錯誤";
                //    break;
                //case ORDER_RETURN_CODE.ORDERNO_EMPTY_ERROR:
                //    msg = "刪改單，請輸入委託書號";
                //    break;
                case ORDER_RETURN_CODE.NO_SPEEDY_AUTHORIZE:
                    msg = "本帳號無Speedy下單權限";
                    break;
                case ORDER_RETURN_CODE.ORDERNO_NOT_EMPTY_ERROR:
                    msg = "新單，請勿輸入委託書號";
                    break;
                default:
                    msg = "其他錯誤";
                    break;
            }
            logBox.AppendText(String.Format("{0}", msg));
            logBox.AppendText(Environment.NewLine);
        }
    }
}
