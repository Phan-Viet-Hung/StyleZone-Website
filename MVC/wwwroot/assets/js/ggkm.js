document.getElementById("btnRandom").addEventListener("click", function () {
    const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    let result = "";
    for (let i = 0; i < 12; i++) {
      result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    document.getElementById("maKMInput").value = result;
    document.querySelector(".summary-box p").textContent = code;
  });