[CustomMessages]
openxmlsdk20_title=Open XML SDK 2.0

openxmlsdk20_size=3.85 MB



[Code]
const
	openxmlsdk20_url = 'http://download.microsoft.com/download/B/1/A/B1AD4A8C-6A81-4A12-8ED8-AC9C03745B6A/OpenXMLSDKv2.msi';

procedure openxmlsdk20();
begin
	if (not RegKeyExists(HKCR, 'Installer\Products\67D8D17150F3A5548AFA5C162C769950')) then
		AddProduct('OpenXMLSDKv2.msi',
			'/qb',
			CustomMessage('openxmlsdk20_title'),
			CustomMessage('openxmlsdk20_size'),
			openxmlsdk20_url,
			false, false, false);
end;
