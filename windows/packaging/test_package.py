"""Dependency-free identity/privilege contracts; no installation or OCR simulation."""
import unittest
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
F = "{http://schemas.microsoft.com/appx/manifest/foundation/windows10}"
U = "{http://schemas.microsoft.com/appx/manifest/uap/windows10/10}"
M = "{urn:schemas-microsoft-com:msix.v1}"


class IdentityContracts(unittest.TestCase):
    def xml(self, relative):
        path = ROOT / relative
        self.assertTrue(path.is_file(), f"Missing explicit identity contract: {relative}")
        return ET.parse(path).getroot()

    def test_identity_matches_embedded_manifest_without_changing_portable_settings(self):
        package = self.xml("packaging/identity/AppxManifest.xml")
        identity = package.find(F + "Identity")
        embedded = self.xml("Bluey.Windows/app.ocr.manifest")
        metadata = embedded.find(M + "msix")
        self.assertIsNotNone(metadata)
        self.assertEqual(identity.get("Name"), metadata.get("packageName"))
        self.assertEqual(identity.get("Publisher"), metadata.get("publisher"))
        self.assertEqual(package.find(F + "Applications/" + F + "Application").get("Id"), metadata.get("applicationId"))
        embedded.remove(metadata)
        portable = self.xml("Bluey.Windows/app.manifest")
        self.assertIsNone(portable.find(M + "msix"))
        self.assertEqual(ET.canonicalize(ET.tostring(portable), strip_text=True), ET.canonicalize(ET.tostring(embedded), strip_text=True))

    def test_external_package_keeps_full_trust_paths_and_has_no_extra_capabilities(self):
        package = self.xml("packaging/identity/AppxManifest.xml")
        self.assertEqual("true", package.find(F + "Properties/" + U + "AllowExternalContent").text)
        app = package.find(F + "Applications/" + F + "Application")
        self.assertEqual("win32App", app.get(U + "RuntimeBehavior"))
        self.assertEqual("mediumIL", app.get(U + "TrustLevel"))
        self.assertEqual("Bluey.Windows.exe", app.get("Executable"))
        caps = package.find(F + "Capabilities")
        self.assertEqual({"runFullTrust", "unvirtualizedResources"}, {c.get("Name") for c in caps})
        self.assertEqual("x64", package.find(F + "Identity").get("ProcessorArchitecture"))
        self.assertEqual("10.0.19041.0", package.find(F + "Dependencies/" + F + "TargetDeviceFamily").get("MinVersion"))

    def test_windows10_19045_meets_both_package_and_managed_api_floor(self):
        package = self.xml("packaging/identity/AppxManifest.xml")
        project = self.xml("Bluey.Windows/Bluey.Windows.csproj")
        minimum = package.find(F + "Dependencies/" + F + "TargetDeviceFamily").get("MinVersion")
        supported = project.find("PropertyGroup/SupportedOSPlatformVersion")
        self.assertIsNotNone(supported, "Declare the managed OS floor; SDK target alone is insufficient")
        self.assertEqual(minimum, supported.text)
        self.assertLessEqual(tuple(map(int, minimum.split('.'))), (10, 0, 19045, 0))
        self.assertEqual("net10.0-windows10.0.22000.0", project.find("PropertyGroup/TargetFramework").text)


if __name__ == "__main__":
    unittest.main()
