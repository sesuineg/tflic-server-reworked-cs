namespace TFlic.Models.Authentication;

public record RsaKeysDto(byte[] PublicKeyPkcs1, byte[] PrivateKeyPkcs8);