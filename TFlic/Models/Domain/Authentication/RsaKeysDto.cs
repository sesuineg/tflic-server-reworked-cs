namespace TFlic.Models.Domain.Authentication;

public record RsaKeysDto(byte[] PublicKeyPkcs1, byte[] PrivateKeyPkcs8);