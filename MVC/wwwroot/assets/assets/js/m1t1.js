function generateCode() {
    const length = 12;
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
    let code = '';
    for (let i = 0; i < length; i++) {
      code += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    document.getElementById("maKM").value = code;
    document.querySelector(".summary-box p").textContent = code;
  }