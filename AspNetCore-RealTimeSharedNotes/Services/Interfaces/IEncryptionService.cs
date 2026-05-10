namespace AspNetCore_RealTimeSharedNotes.Services.Interfaces;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
