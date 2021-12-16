using InsuranceIntegration.Models;
using ISAIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace InsuranceIntegration.Controllers
{
    public class ProductsController : ApiController
    {
        AISIntegrationServiceClient client = new AISIntegrationServiceClient();

        public ISAIS_InsuranceContractCompletedResponse GetAllProducts()
        {
            string sPasswordChallenge = null;

            string sUserName = "ergo_test_60001";
            string sPassword = "12345";

            ISAIS_OpenSessionResponse openSessionResponse = client.OpenSession();
            if (openSessionResponse.responseCode != ISAIS_ReturnCode.OK)
            {
                throw new Exception(openSessionResponse.responseMessage);
            }

            sPasswordChallenge = openSessionResponse.PasswordChallenge;
            string sMD5 = Client.GetMD5(sPassword + sPasswordChallenge);

            var userLoginResponse = client.UserLogin(sUserName, sMD5);
            if (userLoginResponse.responseCode != ISAIS_ReturnCode.OK)
            {
                throw new Exception(userLoginResponse.responseMessage);
            }

            var nextToken = userLoginResponse.NextTocken;
            var sessionId = userLoginResponse.SessionID;

            //Проверка состояния сессии
            CheckUserSessionResponse stateSession = client.CheckUserSession(sessionId);
            /*if (stateSession.responseCode != ISAIS_ReturnCode.OK)
                return stateSession;*/

            //Получение списка правил страхования
            ISAIS_GetInsuranceRuleListResponse rules = client.GetInsuranceRuleList(sessionId, nextToken);
            nextToken = rules.NextTocken;

            //Идентификатор страхового продукта
            string insuranceId = "98115000146633414";

            //--------------------------------------------------------------Получение списка вариантов стахования-----------------------------------------------------------------------
            ISAIS_GetInsuranceVariantListResponse variants = client.GetInsuranceVariantList(sessionId, nextToken, insuranceId);
            nextToken = variants.NextTocken;

            //Вариант страхования
            string variantId = "98115630747646408";

            //--------------------------------------------------------------Получение расширеного списка правил страхования-----------------------------------------------------------------------
            /*ISAIS_GetInsuranceRuleExtendedListResponse extendRules = client.GetInsuranceRuleExtendedList(sessionId, nextToken);
            nextToken = extendRules.NextTocken;*/

            //--------------------------------------------------------------Начало заключения договорастрахования-----------------------------------------------------------------------
            ISAIS_InsuranceContractInitializeResponse contract = client.InsuranceContractInitialize(sessionId, nextToken, insuranceId, variantId, ISAIS_CharListProcedureType.StepByStepLoop);
            nextToken = contract.NextTocken;
            //Идентификатор договора
            string contractId = contract.SessionContractID;
            //--------------------------------------------------------------Получение списка характеристик договора-----------------------------------------------------------------------

            ISAIS_SendInsuranceContractCharacteristicValuesResponse sendChar = new ISAIS_SendInsuranceContractCharacteristicValuesResponse();
            List<ISAIS_GetInsuranceContractCharacteristicListResponse> lists = new List<ISAIS_GetInsuranceContractCharacteristicListResponse>();
            ISAIS_GetInsuranceContractCharacteristicListResponse characteristics = new ISAIS_GetInsuranceContractCharacteristicListResponse();
            ISAIS_InsuranceContractCharValue value = new ISAIS_InsuranceContractCharValue();
            do
            {
                lists.Add(characteristics);
                characteristics = client.GetInsuranceContractCharacteristicList(sessionId, nextToken, contractId);
                
                nextToken = characteristics.NextTocken;
                if (characteristics.FurtherCharRequestRequired == ISAIS.ISAIS_FurtherCharRequestRequired.No)
                    break;
                ISAIS_InsuranceContractChar[] chars = characteristics.InsuranceContractCharList;
                //--------------------------------------------------------------Передача значений характеристик договора-----------------------------------------------------------------------
                foreach (var item in chars)
                {
                    value.CharacteristicTypeID = item.CharacteristicTypeID;
                    if (item.CharacteristicTypeID == "100001" || item.CharacteristicTypeID == "100002")
                        value.CharacteristicValue = "20211210 09:00:00";
                    if (item.CharacteristicTypeID == "100003")
                        value.CharacteristicValue = "20220110 23:59:59";
                    if (item.CharacteristicTypeID == "110001")
                        value.CharacteristicValue = "1";
                    if (item.CharacteristicTypeID == "110002")
                        value.CharacteristicValue = "1";
                    if (item.CharacteristicTypeID == "7703103411932")
                        value.CharacteristicValue = "100013";
                    if (item.CharacteristicTypeID == "7703091661236")
                        value.CharacteristicValue = "12";
                    if (item.CharacteristicTypeID == "981150001415584572")
                        value.CharacteristicValue = "0";
                    if (item.CharacteristicTypeID == "981150001415584575")
                        value.CharacteristicValue = "0";
                    if (item.CharacteristicTypeID == "7701470357763")
                        value.CharacteristicValue = "840";
                    if (item.CharacteristicTypeID == "7701470358108")
                        value.CharacteristicValue = "840";
                    if (item.CharacteristicTypeID == "98115000136064698")
                        value.CharacteristicValue = "8";
                    if (item.CharacteristicTypeID == "7701456400852")
                        value.CharacteristicValue = "1";
                    if (item.CharacteristicTypeID == "981150001415584578")
                        value.CharacteristicValue = "0";
                    if (item.CharacteristicTypeID == "98115000136064712")
                        value.CharacteristicValue = "10";
                    if (item.CharacteristicTypeID == "981150001415585008")
                        value.CharacteristicValue = "0";
                    if (item.CharacteristicTypeID == "981150001415584590")
                        value.CharacteristicValue = "1";
                    if (item.CharacteristicTypeID == "981150001415584593")
                        value.CharacteristicValue = "0";
                    if (item.CharacteristicTypeID == "981150001415584596")
                        value.CharacteristicValue = "0";
                    if (item.CharacteristicTypeID == "98115000135723927")
                        value.CharacteristicValue = "0";
                    if (item.CharacteristicTypeID == "98115000137072886")
                        value.CharacteristicValue = "10";
                    if (item.CharacteristicTypeID == "7702247554158")
                        value.CharacteristicValue = "";
                    if (item.CharacteristicTypeID == "200000")
                        value.CharacteristicValue = "7702828465963";
                    if (item.CharacteristicTypeID == "98110468156157200")
                        value.CharacteristicValue = "202002193347182795";
                    if (item.CharacteristicTypeID == "98115000136089401")
                        value.CharacteristicValue = "30001";
                    if (item.CharacteristicTypeID == "981150001415584605")
                        value.CharacteristicValue = "0";
                    if (item.CharacteristicTypeID == "981150001415584608")
                        value.CharacteristicValue = "1";
                    if (item.CharacteristicTypeID == "981150001415584611")
                        value.CharacteristicValue = "0";
                    if (item.CharacteristicTypeID == "981150001415585284")
                        value.CharacteristicValue = "12";
                    if (item.CharacteristicTypeID == "98115000136082788")
                        value.CharacteristicValue = "0";
                    if (item.CharacteristicTypeID == "981150001415584617")
                        value.CharacteristicValue = "0";
                }
                ISAIS_InsuranceContractCharValue[] charValue = { value };
                sendChar = client.SendInsuranceContractCharacteristicValues(sessionId, nextToken, contractId, charValue);
                nextToken = sendChar.NextTocken;
            }
            while (true);

            //--------------------------------------------------------------Получение статуса договора страхования-----------------------------------------------------------------------

            ISAIS_GetInsuranceContractDataStatusResponse status = client.GetInsuranceContractDataStatus(sessionId, nextToken, contractId);
            nextToken = status.NextTocken;

            //--------------------------------------------------------------Получение страхового тарифа и графика платежей-----------------------------------------------------------------------

            ISAIS_GetInsuranceContractTariffResponse tarif = client.GetInsuranceContractTariff(sessionId, nextToken, contractId);
            nextToken = tarif.NextTocken;

            //--------------------------------------------------------------Получение оферты по договору страхования-----------------------------------------------------------------------

            ISAIS_ContractPolis polis = new ISAIS_ContractPolis();
            polis.ContractFormCode = "2РН/2РП";
            polis.ContractNumber = "12010";
            polis.ContractSeries = "EI";
            ISAIS_GetInsuranceContractOfferResponse offer = client.GetInsuranceContractOffer(sessionId, nextToken, contractId, polis);
            nextToken = offer.NextTocken;

            //--------------------------------------------------------------Начало транзакции оплаты по договору-----------------------------------------------------------------------

            ISAIS_InsuranceContractPayTransactionBeginResponse begin_trans = client.InsuranceContractPayTransactionBegin(sessionId, nextToken, contractId, ISAIS_PaymentProcType.Undefined);
            nextToken = begin_trans.NextTocken;

            //--------------------------------------------------------------Получение страхового тарифа и графика платежей-----------------------------------------------------------------------

            tarif = client.GetInsuranceContractTariff(sessionId, nextToken, contractId);
            nextToken = tarif.NextTocken;
            ISAIS_PaymentSchedule[] schedule = tarif.PaymentSchedule;
            var payAmount = schedule[0].PayAmount;
            var tariffCurrency = tarif.ContractTariff.TariffCurrency;
            string contractTransID = begin_trans.ContractTransID;

            //--------------------------------------------------------------Завершение транзакции оплаты части взноса-----------------------------------------------------------------------

            ISAIS_InsuranceContractPayTransactionEndResponse end_trans = client.InsuranceContractPayTransactionEnd(sessionId, nextToken, contractId, contractTransID, payAmount, tariffCurrency, ISAIS_TransactionStatus.Success, "Ok");
            nextToken = end_trans.NextTocken;

            /*//--------------------------------------------------------------Получение статуса договора страхования-----------------------------------------------------------------------
            ISAIS_GetInsuranceContractStatusResponse new_status = client.GetInsuranceContractStatus(sessionId, nextToken, contractId);
            nextToken = status.NextTocken;*/

            //--------------------------------------------------------------Регистрация заключения договора со стороны Клиента-----------------------------------------------------------------------

            ISAIS_InsuranceContractCompletedResponse contractCompleted = client.InsuranceContractCompleted(sessionId, nextToken, contractId, contractTransID);

            return contractCompleted;
        }



        public IHttpActionResult GetProduct(int id)
        {
            return Ok();
        }
    }
}
