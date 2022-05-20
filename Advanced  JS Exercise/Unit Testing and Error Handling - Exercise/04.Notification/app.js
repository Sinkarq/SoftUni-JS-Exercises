function notify(message) {
  const notificationDiv = document.querySelector(`#notification`);
  notificationDiv.textContent = message;
  notificationDiv.style.display = 'block';
  notificationDiv.addEventListener(`click`, (e) => {
    e.target.style.display = 'none';
  })
}