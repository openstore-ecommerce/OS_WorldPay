﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OS_WorldPay.Components
{
    public enum TransactionStatus
    {
        Successful = 0,
        CardReferred = 2,
        CardDeclined = 5,
        DuplicateTransaction = 20,
        Exception = 30,
        NotSpecified = 100
    }
}
