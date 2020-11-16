window.addEventListener('load', () => {
	const loader = document.getElementById('loader')
	loader.classList.add('text-loader-fadeOut')
	const tOut = setTimeout(() => {
		loader.style.display = 'none'
		clearTimeout(tOut)
	})
})
