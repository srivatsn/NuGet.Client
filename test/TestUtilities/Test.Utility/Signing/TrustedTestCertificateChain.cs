// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Test.Utility.Signing
{
    public class TrustedTestCertificateChain : IDisposable
    {
        private readonly IList<StoreCertificate<TestCertificate>> _certificates;

        public IStoreCertificate<TestCertificate> Root => _certificates?.First();
        public IStoreCertificate<TestCertificate> Leaf => _certificates?.Last();

        public TrustedTestCertificateChain(IList<StoreCertificate<TestCertificate>> certificates)
        {
            _certificates = certificates;
        }

        public void Dispose()
        {
            if (_certificates != null)
            {
                foreach (var certificate in _certificates)
                {
                    certificate.Dispose();
                }
            }
        }
    }
}