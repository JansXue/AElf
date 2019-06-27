using System.Threading.Tasks;
using AElf.Contracts.MultiToken.Messages;
using AElf.Contracts.TestKit;
using AElf.Contracts.Treasury;
using AElf.Cryptography.ECDSA;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace AElf.Contracts.EconomicSystem.Tests.BVT
{
    public partial class EconomicSystemTest
    {
        [Fact]
        public async Task Donate_FewELF_Success()
        {
            var keyPair = SampleECKeyPairs.KeyPairs[1];
            await TransferToken(keyPair, EconomicSystemTestConstants.NativeTokenSymbol, 100);
            var stub = GetTreasuryContractStub(keyPair);
            
            var donateResult = await stub.Donate.SendAsync(new DonateInput
            {
                Symbol = EconomicSystemTestConstants.NativeTokenSymbol,
                Amount = 50
            });
            donateResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);

            var userBalance = (await TokenContractStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = Address.FromPublicKey(keyPair.PublicKey),
                Symbol = EconomicSystemTestConstants.NativeTokenSymbol
            })).Balance;
            userBalance.ShouldBe(50);

            var treasuryBalance = await GetCurrentTreasuryBalance();
            treasuryBalance.ShouldBeGreaterThanOrEqualTo(50);
        }
        
        [Fact]
        public async Task Donate_AllELF_Success()
        {
            var keyPair = SampleECKeyPairs.KeyPairs[1];
            await TransferToken(keyPair, EconomicSystemTestConstants.NativeTokenSymbol, 100);
            var stub = GetTreasuryContractStub(keyPair);
            
            var donateResult = await stub.DonateAll.SendAsync(new DonateAllInput
            {
                Symbol = EconomicSystemTestConstants.NativeTokenSymbol
            });
            donateResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);

            var userBalance = (await TokenContractStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = Address.FromPublicKey(keyPair.PublicKey),
                Symbol = EconomicSystemTestConstants.NativeTokenSymbol
            })).Balance;
            userBalance.ShouldBe(0);
            
            var treasuryBalance = await GetCurrentTreasuryBalance();
            treasuryBalance.ShouldBeGreaterThanOrEqualTo(100);
        }
        
        [Fact]
        public async Task Donate_ELF_LessThan_Owned()
        {
            var keyPair = SampleECKeyPairs.KeyPairs[1];
            
            await TransferToken(keyPair, EconomicSystemTestConstants.NativeTokenSymbol, 50);
            var stub = GetTreasuryContractStub(keyPair);
            var donateResult = await stub.Donate.SendAsync(new DonateInput
            {
                Symbol = EconomicSystemTestConstants.NativeTokenSymbol,
                Amount = 100
            });
            donateResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);

            var userBalance = (await TokenContractStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = Address.FromPublicKey(keyPair.PublicKey),
                Symbol = EconomicSystemTestConstants.NativeTokenSymbol
            })).Balance;
            userBalance.ShouldBe(50);
        }
        
        [Fact]
        public async Task Donate_FewOtherToken_Success()
        {
            var keyPair = SampleECKeyPairs.KeyPairs[1];
            
            await TransferToken(keyPair, EconomicSystemTestConstants.ConverterTokenSymbol, 100);
            var stub = GetTreasuryContractStub(keyPair);
            var donateResult = await stub.Donate.SendAsync(new DonateInput
            {
                Symbol = EconomicSystemTestConstants.ConverterTokenSymbol,
                Amount = 50
            });
            donateResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);

            var userBalance = (await TokenContractStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = Address.FromPublicKey(keyPair.PublicKey),
                Symbol = EconomicSystemTestConstants.ConverterTokenSymbol
            })).Balance;
            userBalance.ShouldBe(50);
        }
        
        [Fact]
        public async Task Donate_AllOtherToken_Success()
        {
            var keyPair = SampleECKeyPairs.KeyPairs[1];
            
            await TransferToken(keyPair, EconomicSystemTestConstants.ConverterTokenSymbol, 100);
            var stub = GetTreasuryContractStub(keyPair);
            var donateResult = await stub.DonateAll.SendAsync(new DonateAllInput
            {
                Symbol = EconomicSystemTestConstants.ConverterTokenSymbol
            });
            donateResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);

            var userBalance = (await TokenContractStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = Address.FromPublicKey(keyPair.PublicKey),
                Symbol = EconomicSystemTestConstants.ConverterTokenSymbol
            })).Balance;
            userBalance.ShouldBe(0);
        }

        [Fact]
        public async Task Donate_OtherToken_LessThan_Owned()
        {
            var keyPair = SampleECKeyPairs.KeyPairs[1];
            
            await TransferToken(keyPair, EconomicSystemTestConstants.ConverterTokenSymbol, 50);
            var stub = GetTreasuryContractStub(keyPair);
            var donateResult = await stub.Donate.SendAsync(new DonateInput
            {
                Symbol = EconomicSystemTestConstants.ConverterTokenSymbol,
                Amount = 100
            });
            donateResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);

            var userBalance = (await TokenContractStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = Address.FromPublicKey(keyPair.PublicKey),
                Symbol = EconomicSystemTestConstants.ConverterTokenSymbol
            })).Balance;
            userBalance.ShouldBe(100);
        }
        private async Task TransferToken(ECKeyPair keyPair, string symbol, long amount)
        {
            var toAddress = Address.FromPublicKey(keyPair.PublicKey);
            var transferResult = await TokenContractStub.Transfer.SendAsync(new TransferInput
            {
                To = toAddress,
                Symbol = symbol,
                Amount = amount,
                Memo = "transfer for test"
            });
            transferResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
        }

        private async Task<long> GetCurrentTreasuryBalance()
        {
            var balance = await TreasuryContractStub.GetCurrentTreasuryBalance.CallAsync(new Empty());

            return balance.Value;
        }
    }
}