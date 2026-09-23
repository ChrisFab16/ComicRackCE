# Web Configure Sample - SPA Configure via Host API v1
#
#@Name Web Configure Sample
#@Hook Books
#@Key WebConfigureSample
#@Enabled true
#@Description Opens a message; use Configure for the SPA UI
def WebConfigureSampleBooks(books):
	from System.Windows.Forms import MessageBox
	count = 0 if books is None else books.Length
	MessageBox.Show("Web Configure Sample: %d book(s). Use Configure for SPA UI." % count)

#
#@Key WebConfigureSample
#@Hook ConfigScript
def ConfigureWebConfigureSample():
	ComicRack.ShowWebConfigure()
