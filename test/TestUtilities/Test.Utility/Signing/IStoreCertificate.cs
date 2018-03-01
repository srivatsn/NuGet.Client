// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Cryptography.X509Certificates;

namespace Test.Utility.Signing
{
    // This does not implemement IDisposable to discourage tests from disposing an instance
    // shared across a test collection.
    public interface IStoreCertificate<T>
    {
        X509Certificate2 Certificate { get; }
        T Source { get; }
        StoreLocation StoreLocation { get; }
        StoreName StoreName { get; }
    }
}