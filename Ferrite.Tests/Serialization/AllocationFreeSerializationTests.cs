// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Autofac;
using Autofac.Core.Lifetime;
using Autofac.Extras.Moq;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.currentLayer.storage;
using Ferrite.TL.currentLayer.upload;
using Ferrite.TL.mtproto;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer146;
using Ferrite.TL.slim.layer146.storage;
using Ferrite.TL.slim.layer146.upload;
using Ferrite.TL.slim.mtproto;
using Ferrite.Utils;
using Moq;
using Xunit;
using ReqDhParams = Ferrite.TL.mtproto.ReqDhParams;
using ResPQ = Ferrite.TL.mtproto.ResPQ;
using RpcResult = Ferrite.TL.slim.mtproto.RpcResult;
using VectorOfDouble = Ferrite.TL.VectorOfDouble;
using VectorOfInt = Ferrite.TL.VectorOfInt;
using VectorOfLong = Ferrite.TL.VectorOfLong;

namespace Ferrite.Tests.Serialization;

public class AllocationFreeSerializationTests
{
    [Fact]
    public void ReqDhParams_Should_Serialize()
    {
        var container = BuildContainer();
        var tmp = container.Resolve<ReqDhParams>();
        tmp.Nonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
        tmp.ServerNonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
        tmp.P = RandomNumberGenerator.GetBytes(4);
        tmp.Q = RandomNumberGenerator.GetBytes(4);
        tmp.EncryptedData = RandomNumberGenerator.GetBytes(278);
        tmp.PublicKeyFingerprint = 123741692374192L;
        byte[] data = tmp.TLBytes.ToArray();
        using Ferrite.TL.slim.mtproto.ReqDhParams reqDhParams = new((byte[])tmp.Nonce,
                (byte[])tmp.ServerNonce, tmp.P, tmp.Q, tmp.PublicKeyFingerprint, tmp.EncryptedData);
        Assert.Equal(data.Length, reqDhParams.Length);
        Assert.Equal(data, reqDhParams.ToReadOnlySpan().ToArray());
    }
    [Fact]
    public void ResPq_Should_Serialize()
    {
        var container = BuildContainer();
        var tmp = container.Resolve<ResPQ>();
        tmp.Nonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
        tmp.ServerNonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
        tmp.Pq = RandomNumberGenerator.GetBytes(8);
        var fingerprints = new VectorOfLong(3);
        fingerprints.Add(123416662344445L);
        fingerprints.Add(734657345673634L);
        fingerprints.Add(923874923784422L);
        tmp.ServerPublicKeyFingerprints = fingerprints;
        byte[] data = tmp.TLBytes.ToArray();
        var fingerprints2 = new Ferrite.TL.slim.VectorOfLong();
        foreach (var f in fingerprints)
        {
            fingerprints2.Append(f);
        }
        using Ferrite.TL.slim.mtproto.ResPQ value = new((byte[])tmp.Nonce,
                (byte[])tmp.ServerNonce, tmp.Pq, fingerprints2);
        Assert.Equal(data.Length, value.Length);
        Assert.Equal(data, value.ToReadOnlySpan().ToArray());
    }
    [Fact]
    public void Vector_Should_Serialize()
    {
        var container = BuildContainer();
        var vecTmp = new Vector<ReqDhParams>(container.Resolve<ITLObjectFactory>());
        for (int i = 0; i < 10; i++)
        {
            var tmp = container.Resolve<ReqDhParams>();
            tmp.Nonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
            tmp.ServerNonce = (Ferrite.TL.Int128)RandomNumberGenerator.GetBytes(16);
            tmp.P = RandomNumberGenerator.GetBytes(4);
            tmp.Q = RandomNumberGenerator.GetBytes(4);
            tmp.EncryptedData = RandomNumberGenerator.GetBytes(33);
            tmp.PublicKeyFingerprint = 123741692374192L+i;
            vecTmp.Add(tmp);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        var vec = new Ferrite.TL.slim.Vector();
        foreach (var tmp in vecTmp)
        {
            using var reqDhParams = new Ferrite.TL.slim.mtproto.ReqDhParams((byte[])tmp.Nonce,
                (byte[])tmp.ServerNonce, tmp.P, tmp.Q, tmp.PublicKeyFingerprint, tmp.EncryptedData);
            vec.AppendTLObject(reqDhParams.ToReadOnlySpan());
            
        }
        var actual = vec.ToReadOnlySpan().ToArray();
        Assert.Equal(actual.Length, vec.Length);
        Assert.Equal(data, actual);
    }
    [Fact]
    public void VectorOfBytes_Should_Serialize()
    {
        var vecTmp = new VectorOfBytes();
        for (int i = 0; i < 16; i++)
        {
            vecTmp.Add(RandomNumberGenerator.GetBytes(16));
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        var vec = new Ferrite.TL.slim.Vector();
        foreach (var tmp in vecTmp)
        {
            vec.AppendTLBytes(tmp);
        }
        
        var actual = vec.ToReadOnlySpan().ToArray();
        Assert.Equal(data, actual);
    }
    [Fact]
    public void VectorOfInt_Should_Serialize()
    {
        var vecTmp = new VectorOfInt();
        for (int i = 0; i < 100; i++)
        {
            vecTmp.Add(i);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        var vec = new Ferrite.TL.slim.VectorOfInt();
        foreach (var tmp in vecTmp)
        {
            vec.Append(tmp);
        }
        
        var actual = vec.ToReadOnlySpan().ToArray();
        Assert.Equal(data, actual);
    }
    [Fact]
    public void VectorOfLong_Should_Serialize()
    {
        var vecTmp = new VectorOfLong();
        for (int i = 0; i < 100; i++)
        {
            vecTmp.Add(i);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        var vec = new Ferrite.TL.slim.VectorOfLong();
        foreach (var tmp in vecTmp)
        {
            vec.Append(tmp);
        }
        
        var actual = vec.ToReadOnlySpan().ToArray();
        Assert.Equal(data, actual);
    }
    [Fact]
    public void VectorOfDouble_Should_Serialize()
    {
        var vecTmp = new VectorOfDouble();
        for (int i = 0; i < 100; i++)
        {
            vecTmp.Add(i+0.3);
        }
        byte[] data = vecTmp.TLBytes.ToArray();
        var vec = new Ferrite.TL.slim.VectorOfDouble();
        foreach (var tmp in vecTmp)
        {
            vec.Append(tmp);
        }
        
        var actual = vec.ToReadOnlySpan().ToArray();
        Assert.Equal(data, actual);
    }
    [Fact]
    public void FluentAPI_Should_Build_server_DH_inner_data()
    {
        var nonce = RandomNumberGenerator.GetBytes(16);
        var serverNonce = RandomNumberGenerator.GetBytes(16);
        int g = 3;
        var dhPrime = RandomNumberGenerator.GetBytes(256);
        var ga = RandomNumberGenerator.GetBytes(8);
        int serverTime = (int)DateTimeOffset.Now.ToUnixTimeSeconds();

        using var actual = Ferrite.TL.slim.mtproto.ServerDhInnerData.Builder()
            .with_nonce(nonce)
            .with_server_nonce(serverNonce)
            .with_g(g)
            .with_dh_prime(dhPrime)
            .with_g_a(ga)
            .with_server_time(serverTime)
            .Build();
        
        using var expected = new Ferrite.TL.slim.mtproto.ServerDhInnerData(
            nonce, 
            serverNonce, 
            g, 
            dhPrime, 
            ga, 
            serverTime);
        
        Assert.Equal(nonce, actual.nonce.ToArray());
        Assert.Equal(serverNonce, actual.server_nonce.ToArray());
        Assert.Equal(g, actual.g);
        Assert.Equal(dhPrime, actual.dh_prime.ToArray());
        Assert.Equal(ga, actual.g_a.ToArray());
        Assert.Equal(serverTime, actual.server_time);
        
        Assert.Equal(expected.ToReadOnlySpan().ToArray(), 
            actual.ToReadOnlySpan().ToArray());
        
        Assert.Equal(expected.TLBytes!.Value.AsSpan().ToArray(), 
            actual.TLBytes!.Value.AsSpan().ToArray());
    }
    [Fact]
    public void FluentAPI_Should_Build_future_salts()
    {
        long reqMsgId = 13579;
        int now = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
        VectorBare salts = new VectorBare();
        int validSince = now;
        int validUntil = now + 1800;
        long saltValue = Random.Shared.NextInt64();
        using Ferrite.TL.slim.mtproto.FutureSalt salt1 = new(validSince, validUntil, saltValue);
        salts.Append(salt1.ToReadOnlySpan());
        using Ferrite.TL.slim.mtproto.FutureSalt salt2 = new(validSince+7200, 
            validUntil+7200, saltValue + 7200);
        salts.Append(salt2.ToReadOnlySpan());

        using var actual = Ferrite.TL.slim.mtproto.FutureSalts.Builder()
            .with_req_msg_id(reqMsgId)
            .with_now(now)
            .with_salts(salts)
            .Build();

        using var expected = new Ferrite.TL.slim.mtproto.FutureSalts(reqMsgId, now, salts);

        Assert.Equal(expected.ToReadOnlySpan().ToArray(), 
            actual.ToReadOnlySpan().ToArray());
        
        Assert.Equal(expected.TLBytes!.Value.AsSpan().ToArray(), 
            actual.TLBytes!.Value.AsSpan().ToArray());
    }
    [Fact]
    public void FluentAPI_Should_Build_resPQ()
    {
        var nonce = RandomNumberGenerator.GetBytes(16);
        var serverNonce = RandomNumberGenerator.GetBytes(16);
        var pq = RandomNumberGenerator.GetBytes(8);
        Ferrite.TL.slim.VectorOfLong fingerprints = new Ferrite.TL.slim.VectorOfLong();
        fingerprints.Append(135790L);
        fingerprints.Append(246810L);

        using var actual = Ferrite.TL.slim.mtproto.ResPQ.Builder()
            .with_nonce(nonce)
            .with_server_nonce(serverNonce)
            .with_pq(pq)
            .with_server_public_key_fingerprints(fingerprints)
            .Build();

        using var expected = new Ferrite.TL.slim.mtproto.ResPQ(nonce, serverNonce, pq, fingerprints);

        Assert.Equal(expected.ToReadOnlySpan().ToArray(), 
            actual.ToReadOnlySpan().ToArray());
        
        Assert.Equal(expected.TLBytes!.Value.AsSpan().ToArray(), 
            actual.TLBytes!.Value.AsSpan().ToArray());
    }
    private static IContainer BuildContainer()
    {
        var logger = new Mock<ILogger>();
        Dictionary<long, byte[]> authKeys = new Dictionary<long, byte[]>();
        Dictionary<long, byte[]> sessions = new Dictionary<long, byte[]>();
        var proto = new Mock<IMTProtoService>();
        proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), It.IsAny<byte[]>())).ReturnsAsync((long a, byte[] b) =>
        {
            authKeys.Add(a, b);
            return true;
        });
        proto.Setup(x => x.GetAuthKeyAsync(It.IsAny<long>())).ReturnsAsync((long a) =>
        {
            if (!authKeys.ContainsKey(a))
            {
                return new byte[0];
            }
            return authKeys[a];
        });

        Dictionary<long, byte[]> authKeys2 = new Dictionary<long, byte[]>();
        var tl = Assembly.Load("Ferrite.TL");
        var builder = new ContainerBuilder();
        builder.RegisterType<RandomGenerator>().As<IRandomGenerator>();
        builder.RegisterType<KeyProvider>().As<IKeyProvider>();
        builder.RegisterAssemblyTypes(tl)
            .Where(t => t.Namespace == "Ferrite.TL.mtproto")
            .AsSelf();
        builder.Register(_ => new Ferrite.TL.Int128());
        builder.Register(_ => new Int256());
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterMock(proto);
        builder.RegisterMock(logger);
        var container = builder.Build();
        return container;
    }

    [Fact]
    public void Should_SerializeFileHeader()
    {
        using var jpeg = new FileJpeg();
        using var file = File_.Builder()
            .with_type(jpeg.ToReadOnlySpan())
            .with_mtime(0)
            .Build();
        using var rpcResult = RpcResult.Builder()
            .with_req_msg_id(0)
            .with_result(file.ToReadOnlySpan())
            .Build();
        var actual = rpcResult.ToReadOnlySpan()[..24];
        var rpcResult2 = new Ferrite.TL.mtproto.RpcResult(null);
        rpcResult2.ReqMsgId = 0;

        byte[] expected = new byte[]
        {
            1, 109, 92, 243, 0, 0, 0, 0, 0, 0, 0, 0,
            213, 24, 106, 9, 14, 254, 126, 0, 0, 0, 0, 0
        };
        Assert.Equal(expected, actual.ToArray());
    }

}