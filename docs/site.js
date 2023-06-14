window.addEventListener('DOMContentLoaded', function() {
    var images = [
      'imgs/first/WIAFN.jpg',
      'imgs/first/image2.png',
      'imgs/first/image.png'
    ];
    var currentIndex = 0;
    var slider = document.querySelector('.slider');
    
    function changeImage() {
      slider.innerHTML = '';
      var img = document.createElement('img');
      img.src = images[currentIndex];
      img.alt = 'Slider Image';
      slider.appendChild(img);
      currentIndex = (currentIndex + 1) % images.length;
      setTimeout(changeImage, 3000);
    }
    
    changeImage();
  });