using System;
using System.Security.Cryptography;
using System.Text;
public class id_generator {
  private static int _counter;

  public static string item_id() {
    byte[] bytes = new byte[12];

    int ts = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    bytes[0] = (byte)(ts >> 24);
    bytes[1] = (byte)(ts >> 16);
    bytes[2] = (byte)(ts >> 8);
    bytes[3] = (byte)(ts);

    RandomNumberGenerator.Fill(bytes.AsSpan(4, 5));

    int c = (_counter++) & 0xFFFFFF;
    bytes[9] = (byte)(c >> 16);
    bytes[10] = (byte)(c >> 8);
    bytes[11] = (byte)c;

    var sb = new StringBuilder(24);
    foreach (var b in bytes)
      sb.Append(b.ToString("x2"));

    return sb.ToString();
  }
}