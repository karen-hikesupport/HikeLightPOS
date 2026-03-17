
namespace HikePOS.Services
{
	public interface IGenerateBarcode
	{
		byte[] DoGenerateBarcode(string barcode);
	}
}
