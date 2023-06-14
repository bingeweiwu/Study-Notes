using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuleManager
{
    public class ClientCallBackImplements : ISulpHurWCFServiceCallback
    {
        public MS.Internal.SulpHur.UICompliance.ForegroundData getForegroundData()
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BegingetForegroundData(AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public MS.Internal.SulpHur.UICompliance.ForegroundData EndgetForegroundData(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public MS.Internal.SulpHur.UICompliance.CapturedData capturedData()
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BegincapturedData(AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public MS.Internal.SulpHur.UICompliance.CapturedData EndcapturedData(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
    }
}
