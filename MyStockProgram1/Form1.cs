using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyStockProgram1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Click은 이벤트 속성 (에 +=로 이벤트 메서드 추가)
            loginButton.Click += LoginButton_Click;

            codeListButton.Click += CodeListButton_Click;

            requestButton.Click += RequestButton_Click;

            trRequestButton.Click += Button_Click;

            // 로그인 시도에 대한 이벤트 속성
            axKHOpenAPI1.OnEventConnect += API_OnEventConnect;

            // 서버로부터 (요청했던) TR데이터를 전달받았을 때
            // OnReceiveTrData의 Delegate 속성과 동일한 속성을 갖는 메서드를 이벤트 함수로 등록해야함 (반환 타입 & 파라미터 타입 등)
            axKHOpenAPI1.OnReceiveTrData += API_OnReceiveTrData;
        }

        // 로그인 버튼 클릭시 발생하는 이벤트 메서드
        private void LoginButton_Click(object sender, EventArgs e)   // 이벤트 메서드는 두 가지 파라미터가 필요함
        {
            Console.WriteLine("버튼이 눌렸습니다.");
            axKHOpenAPI1.CommConnect();     // 로그인창 띄우는 명령어
        }

        // 로그인 시도시 발생하는 이벤트 메서드
        private void API_OnEventConnect(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            Console.WriteLine("사용자가 로그인을 시도했습니다.");

            if (e.nErrCode == 0)
            {
                Console.WriteLine("{0} -> 로그인에 성공했습니다.", e.nErrCode);
                string user_id = axKHOpenAPI1.GetLoginInfo("USER_ID");                      // 사용자 아이디
                string user_name = axKHOpenAPI1.GetLoginInfo("USER_NAME");                  // 사용자 이름
                string account_cnt = axKHOpenAPI1.GetLoginInfo("ACCOUNT_CNT");              // 보유계좌수
                string[] account_list = axKHOpenAPI1.GetLoginInfo("ACCLIST").Split(';');    // 보유계좌의 계좌번호
                string server_type = axKHOpenAPI1.GetLoginInfo("GetServerGubun");           // 접속서버 구분 (1: 모의투자, 나머지: 실서버)

                Console.WriteLine("사용자 ID\t {0}", user_id);
                Console.WriteLine("사용자 이름\t {0}", user_name);
                Console.WriteLine("계좌수\t\t {0}개", account_cnt);

                for (int i = 0; i < Convert.ToInt32(account_cnt); i++)
                {
                    Console.WriteLine("계좌번호 {0}\t {1}", i+1, account_list[i]);
                }

                if (server_type == 1.ToString())
                {

                    Console.WriteLine("서버 구분\t [{0}] 모의투자 서버", server_type);
                }
                else
                {
                    Console.WriteLine("서버 구분\t {0} 실서버", server_type);
                }
            }
            else
            {
                Console.WriteLine("로그인에 실패했습니다.");
                switch (e.nErrCode) {
                    case 100:
                        Console.WriteLine("에러코드 {0} -> 사용자 정보교환 실패", e.nErrCode);
                        break;
                    case 101:
                        Console.WriteLine("에러코드 {0} -> 서버접속 실패", e.nErrCode);
                        break;
                    case 102:
                        Console.WriteLine("에러코드 {0} -> 버전처리 실패", e.nErrCode);
                        break;
                }
            }
        }

        // 종목 코드 버튼 클릭시 발생하는 이벤트 메서드
        private void CodeListButton_Click(object sender, EventArgs e)
        {
            /*
             * [시장구분값] (국내 주식 시장별 종목코드를 ;로 구분해서 전달)
             * 0 : 장내
             * 3 : ELW
             * 4 : 뮤추얼펀드
             * 5 : 신주인수권
             * 6 : 리츠
             * 8 : ETF
             * 9 : 하이얼펀드
             * 10 : 코스닥
             * 30 : K-OTC
             * 50 : KONEX
             * NULL : 전체 시장코드
             */

            // 종목 코드 반환
            string codeList = axKHOpenAPI1.GetCodeListByMarket(null);
            string[] codeArray = codeList.Split(';');

            foreach(string code in codeArray)
            {
                // 종목코드 입력하면 종목명 주는 API 함수 사용
                string itemName = axKHOpenAPI1.GetMasterCodeName(code);
                Console.WriteLine("{0} {1}", code, itemName);
            }

            Console.WriteLine(codeList);
        }

        private void RequestButton_Click(object sender, EventArgs e)
        {
            string code = textBox.Text;
            axKHOpenAPI1.SetInputValue("종목코드", code);
            axKHOpenAPI1.CommRqData("주식정보요청", "opt10001", 0, "1000");
        }

        // 버튼 클릭 이벤트 함수는 Button_Click 하나로 만들고, sender가 누구냐로 상황에 맞는 메서드 호출
        private void Button_Click(object sender, EventArgs e)
        {
            if (sender.Equals(trRequestButton))
            {
                Console.WriteLine("trRequestButton이 클릭됨");
                string code = codeTextBox.Text;

                axKHOpenAPI1.SetInputValue("종목코드", code);
                int result = axKHOpenAPI1.CommRqData("종목기본정보요청", "opt10001", 0, "1001");

                if(result == 0)
                {
                    // 종목기본정보요청TR 요청 성공
                    Console.WriteLine("종목조회요청 성공");
                }
                else
                {
                    // 요청 실패
                    Console.WriteLine("종목조회요청 실패");
                }
            }
        }

        // #2 58:15
        // 서버로 요청을 보내고, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e가 요청시 입력한 조건 데이터를 받아 axKHOpenAPI1.OnReceiveTrData 이벤트가 발생했을 때 호출되는 이벤트 함수
        // AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e가 받은 "요청시 입력 조건 데이터"를 바탕으로 원하는 데이터(현재가, 거래량 등)을 axKHOpenAPI1.GetCommData를 통해 받아옴 
        private void API_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sRQName.Equals("주식정보요청"))
            {
                string price = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "현재가");
                string volume = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "거래량");
                string rangeRate = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "등락율");

                Console.WriteLine("현재가 = {0}", price);
                Console.WriteLine("거래량 = {0}", volume);
                Console.WriteLine("등락률 = {0}", rangeRate);
            }
            else if (e.sRQName.Equals("종목기본정보요청"))
            {
                Console.WriteLine("e.sTrCode {0}", e.sTrCode);          // TR이름 ("opt10001")
                Console.WriteLine("e.sRQName {0}", e.sRQName);          // 사용자 구분명 ("종목기본정보요청")
                Console.WriteLine("e.sScrNo {0}", e.sScrNo);            // 화면번호 ("1001")
                //Console.WriteLine("e.sRecordName {0}", e.sRecordName);  // 레코드 이름
                Console.WriteLine("e.sPrevNext {0}", e.sPrevNext);      // 연속조회 유무를 판단하는 값 0: 연속(주가조회) 데이터 없음, 1: 연속(주가조회) 데이터 있음

                string price = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "현재가");
                string volume = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "거래량");
                string rangeRate = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "등락율");

                Console.WriteLine("현재가 = {0}", price);
                Console.WriteLine("거래량 = {0}", volume);
                Console.WriteLine("등락률 = {0}", rangeRate);
            }
        }

    }
}
