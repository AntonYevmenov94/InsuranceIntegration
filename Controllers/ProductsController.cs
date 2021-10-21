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

        public ISAIS_GetInsuranceContractCharacteristicListResponse GetAllProducts()
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
            ISAIS_GetInsuranceContractCharacteristicListResponse characteristics = client.GetInsuranceContractCharacteristicList(sessionId, nextToken, contractId);
            nextToken = characteristics.NextTocken;

            //ISAIS_InsuranceContractChar[] charVal = characteristics.InsuranceContractCharList;
            /*List<ISAIS_InsuranceContractCharValue> value = new List<ISAIS_InsuranceContractCharValue>();
            foreach (ISAIS_InsuranceContractChar item in charVal)
            {
                value.Add(new ISAIS_InsuranceContractCharValue { CharacteristicTypeID = item.CharacteristicTypeID, CharacteristicValue = item.CharDefaultValue });
            }*/


            //--------------------------------------------------------------Передача значений характеристик договора-----------------------------------------------------------------------

            //var sendChar = client.SendInsuranceContractCharacteristicValues(sessionId, nextToken, contractId, value.ToArray());

            //--------------------------------------------------------------Получение статуса договора страхования-----------------------------------------------------------------------
            //ISAIS_GetInsuranceContractDataStatusResponse status = client.GetInsuranceContractDataStatus(sessionId, nextToken, contractId);



            return characteristics;
        }

        public IHttpActionResult GetProduct(int id)
        {
            return Ok();
        }
    }
}
