using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using System.ServiceModel;
namespace WebServicePDL
{
    [ServiceContract(Namespace = "http://automatagrader.com/")]
    interface IService1
    {
        [OperationContract(Name = "GenerateProblemBestIn")]
        public XElement GenerateProblemBestIn(XElement type, XElement minDiff, XElement maxDiff);
    }
}
