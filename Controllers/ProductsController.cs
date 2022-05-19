using InsuranceIntegration.Models;
using ISAIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Collections.Specialized;
using System.Web.Http.Controllers;
using System.Web;

namespace InsuranceIntegration.Controllers
{
    public class ProductsController : ApiController
    {
        AISIntegrationServiceClient client = new AISIntegrationServiceClient();
        [HttpPost]
        public string GetAllProducts()
        {
            HttpContextWrapper postParams = (HttpContextWrapper)Request.Properties["MS_HttpContext"];
            var parameters = postParams.Request.Form;

            //Проверка параметров запроса
            /*if (parameters.Count < 30)
                return "Введены не все параметры";*/

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

            //--------------------------------------------------------------Получение списка вариантов стахования-----------------------------------------------------------------------
            var userLoginResponse = client.UserLogin(sUserName, sMD5);
            if (userLoginResponse.responseCode != ISAIS_ReturnCode.OK)
            {
                throw new Exception(userLoginResponse.responseMessage);
            }

            var nextToken = userLoginResponse.NextTocken;
            var sessionId = userLoginResponse.SessionID;

            //--------------------------------------------------------------Проверка состояния сессии--------------------------------------------------------------
            CheckUserSessionResponse stateSession = client.CheckUserSession(sessionId);
            /*if (stateSession.responseCode != ISAIS_ReturnCode.OK)
                return stateSession;*/

            //--------------------------------------------------------------Получение списка правил страхования--------------------------------------------------------------
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

                    //Дата заключения договора и дата начала действия договора
                    if (item.CharacteristicTypeID == "100001" || item.CharacteristicTypeID == "100002")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                        //value.CharacteristicValue = "20211216 09:00:00";
                    
                    //Дата окончания действия договора
                    if (item.CharacteristicTypeID == "100003")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                        //value.CharacteristicValue = "20220110 23:59:59";
                    
                    //Тип лица страхователя (1-Физическое лицо, 2-Индивидуальный предприниматель, 3-Юридическое лицо)
                    if (item.CharacteristicTypeID == "110001")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "1";

                    //Идентификатор лица страхователя
                    if (item.CharacteristicTypeID == "112000002000000002")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "1";

                    //Ассистанс(100008-BALT ASSISTANCE LTD, 100010-Kaliptus Assistance, 100013-тест)
                    if (item.CharacteristicTypeID == "7703103411932")
                        //value.CharacteristicValue = "100013";

                    //Вводимый срок пребывания
                    if (item.CharacteristicTypeID == "7703091661236")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "12";

                    //Единовременное страховании группы лиц с равными условиями(1-Да, 0-Нет)
                    if (item.CharacteristicTypeID == "981150001415584572")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "0";

                    //Страхование сотрудников крупных фирм, холдингов, групп компаний, выступающих Страхователями(1-Да, 0-Нет)
                    if (item.CharacteristicTypeID == "981150001415584575")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "0";

                    //Валюта страхования(978-EUR, 840-USD)
                    if (item.CharacteristicTypeID == "7701470357763")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "840";

                    //Валюта оплаты взноса(810-RUB, 978-EUR, 840-USD, 933-BYN)
                    if (item.CharacteristicTypeID == "7701470358108")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "840";

                    //Территория действия (группа стран) договора страхования(6-Шенген, 8-Все страны мира, 7-Все страны, за исключением США, Канады, Австралии, Израиля)
                    if (item.CharacteristicTypeID == "98115000136064698")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "8";

                    //Режим оплаты взноса (группа стран) договора страхования(101-Ежегодно, 1-Единовременно)
                    if (item.CharacteristicTypeID == "7701456400852")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "1";

                    //Оплата посредством банковских платежных карточек премиум класса (1-Да, 0-Нет)
                    if (item.CharacteristicTypeID == "981150001415584578")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "0";

                    //Постоянный клиент (30-Более 2-х раз, 20-Повторное обращение, 10-Нет)
                    if (item.CharacteristicTypeID == "98115000136064712")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "10";

                    //Страхователь - сотрудник ЮЛ-Страхователя ЗАО СК Евроинс (1-Да, 0-Нет)
                    if (item.CharacteristicTypeID == "981150001415585008")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "0";

                    //У страхователя есть действующие договоры по другим видам в ЗАСО «Евроинс» (5-по 1 виду, 6-по 2 видам, 7-по 3 и более видам, 1-Нет)
                    if (item.CharacteristicTypeID == "981150001415584590")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "1";

                    //Заключение договоров страхования на сроки 1-5 дней на территории визовых центров в Республике Беларусь (1-Да, 0-Нет)
                    if (item.CharacteristicTypeID == "981150001415584593")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "0";

                    //Заключение генерального соглашения (1-Да, 0-Нет)
                    if (item.CharacteristicTypeID == "981150001415584596")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "0";

                    //Самостоятельное (без посредника) обращение Страхователя для заключения договора страхования (1-Да, 0-Нет)
                    if (item.CharacteristicTypeID == "98115000135723927")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "0";

                    //Франшиза (2-Безусловная, 10-Без франшизы)
                    if (item.CharacteristicTypeID == "98115000137072886")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "10";

                    //Иные условия договора страхования
                    if (item.CharacteristicTypeID == "7702247554158")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "";

                    //Тип объекта страхования(7702828465963-Физическое лицо)
                    if (item.CharacteristicTypeID == "200000")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "7702828465963";

                    //Идентификатор лица
                    if (item.CharacteristicTypeID == "98110468156157200")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "202002193347182795";

                    //Страховая сумма (в валюте страхования) (30001-30 000, 60000-60 000, 100000-100 000)
                    if (item.CharacteristicTypeID == "98115000136089401")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "30001";

                    //Лица, выезжающие для занятий спортом на профессиональном уровне (1-Да, 0-Нет)
                    if (item.CharacteristicTypeID == "981150001415584605")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "0";

                    //Занятие активным отдыхом (в том числе экстремальный спорт) (1-Да, 3-Без повышенного риска, 4-С повышенным риском)
                    if (item.CharacteristicTypeID == "981150001415584608")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "1";

                    //Лица, отправляющиеся к близким родственникам (1-Да, 0-Нет)
                    if (item.CharacteristicTypeID == "981150001415584611")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "0";

                    //Работа по найму (кроме водителей международников) (10-офисные работники, работники интеллектуального труда, 12-Нет, 11-работники, профессии которых сопряжены с физическими нагрузкам, повышенным риском для жизни и здоровья (кроме водителей, осуществляющих международные перевозки))
                    if (item.CharacteristicTypeID == "981150001415585284")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "12";

                    //Профессиональные водители, международные перевозки (1-Да, 0-Нет)
                    if (item.CharacteristicTypeID == "98115000136082788")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "0";

                    //Студенты, проходящих обучение за рубежом, при заключении договора страхования с продолжительностью поездки 30 дней и более (1-Да, 0-Нет)
                    if (item.CharacteristicTypeID == "981150001415584617")
                        value.CharacteristicValue = parameters.Get(item.CharacteristicTypeID);
                    //value.CharacteristicValue = "0";
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
            polis.ContractNumber = "12022";
            polis.ContractSeries = "EI";
            ISAIS_GetInsuranceContractOfferResponse offer = client.GetInsuranceContractOffer(sessionId, nextToken, contractId, polis);
            nextToken = offer.NextTocken;

            //--------------------------------------------------------------Начало транзакции оплаты по договору-----------------------------------------------------------------------

            ISAIS_InsuranceContractPayTransactionBeginResponse begin_trans = client.InsuranceContractPayTransactionBegin(sessionId, nextToken, contractId, ISAIS_PaymentProcType.PaymentConfirmation);
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

            return contractCompleted.ToString();
        }



        public IHttpActionResult GetProduct(int id)
        {
            return Ok();
        }
    }
}
