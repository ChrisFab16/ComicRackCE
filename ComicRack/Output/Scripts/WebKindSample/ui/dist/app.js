(async function () {
  const infoEl = document.getElementById("hostInfo");
  const booksEl = document.getElementById("books");
  const noteEl = document.getElementById("note");

  function applyTheme(theme) {
    if (!theme) return;
    if (theme.backColor) document.documentElement.style.setProperty("--bg", theme.backColor);
    if (theme.foreColor) document.documentElement.style.setProperty("--fg", theme.foreColor);
  }

  try {
    const info = await HostApi.call("host.getInfo");
    infoEl.textContent =
      "Host " + info.productVersion + " · API v" + info.apiVersion + " · " + info.pluginId + " " + info.pluginVersion;

    const theme = await HostApi.call("host.getTheme");
    applyTheme(theme);

    const cfg = await HostApi.call("host.config.get");
    noteEl.value = cfg && cfg.config ? cfg.config : "";

    let booksPayload = await HostApi.call("host.getSelectedBooks", { limit: 20 });
    if (!booksPayload.books || booksPayload.books.length === 0) {
      booksPayload = await HostApi.call("host.getLibraryBooks", { limit: 20 });
    }
    booksEl.innerHTML = "";
    (booksPayload.books || []).forEach(function (b) {
      const li = document.createElement("li");
      li.textContent = b.caption || b.id;
      booksEl.appendChild(li);
    });
    if (!booksEl.children.length) {
      const li = document.createElement("li");
      li.textContent = "No books selected or in library.";
      booksEl.appendChild(li);
    }
  } catch (err) {
    infoEl.textContent = "Host bridge error: " + (err.message || JSON.stringify(err));
  }

  document.getElementById("btnSave").addEventListener("click", async function () {
    await HostApi.call("host.config.set", { config: noteEl.value });
  });
  document.getElementById("btnReload").addEventListener("click", function () {
    HostApi.call("host.ui.reload");
  });
  document.getElementById("btnClose").addEventListener("click", function () {
    HostApi.call("host.ui.close", { dialogResult: "ok" });
  });
})();
