window.addEventListener('load', () => {
	const jsScript = document.createElement('script')
	jsScript.src = 'https://cdn.jsdelivr.net/npm/vue@2'

	document.body.appendChild(jsScript)

	jsScript.addEventListener('load', () => {
		const jsScript2 = document.createElement('script')
		jsScript.src2 = 'https://unpkg.com/navigator.sendbeacon'

		document.body.appendChild(jsScript2)

		mainStart()
	})
})

function mainStart() {
	var limitSizeUpload = 30000000 //30Mb
	let controller
	let swBeacom = true

	const defaultData = {
		cards: [],
		loading: false,
		btnText: 'Eliminar Seleccionadas',
		downloadable: false,
		modal: false,
		currentSelected: null,
		message: '',
		file: {
			name: '',
			dir: '',
			numPages: 0,
			size: 0
		}
	}
	var vm = new Vue({
		el: '#app',
		data: {
			...defaultData
		},
		mounted() {
			document.getElementById('submit').addEventListener('click', this.onSubmit)
			document.body.style.overflow = 'auto'
			window.addEventListener('beforeunload', (e) => this.onUnloadHandler(e))
			if (window.performance) {
			}
			if (performance.navigation.type == performance.navigation.TYPE_RELOAD) {
				sessionStorage.removeItem('data')
			} else {
			}

			this.initData()
		},

		watch: {
			loading(val, oldVal) {
				if (val) {
					document.body.style.overflow = 'hidden'
				} else {
					document.body.style.overflow = 'auto'
				}
			},
			modal(val, oldVal) {
				if (val) {
					document.body.style.overflow = 'hidden'
				} else {
					document.body.style.overflow = 'auto'
				}
			}
		},
		computed: {
			numCardsSelected() {
				return (this.cards && this.cards.filter((card) => card.selected).length) || 0
			},
			subTiTle() {
				return this.cards.length > 0 ? 'Selecciona las paginas que deseas eliminar' : 'Carga un documento pdf'
			}
		},
		methods: {
			onUnloadHandler(e) {
				if (swBeacom) {
					const form = document.getElementById('my-form-post')
					let data = new FormData(form)
					if (controller) {
						controller.abort()
					}

					navigator.sendBeacon('/Index?handler=RemoveDirectory', data)
				}
			},

			initData() {
				const loader = document.getElementById('loader')
				loader.classList.add('text-loader-fadeOut')

				const tOut = setTimeout(() => {
					loader.style.display = 'none'
					clearTimeout(tOut)
				})

				let dataTemp
				const dataLocal = sessionStorage.getItem('data')
				if (dataLocal) {
					dataTemp = JSON.parse(dataLocal)
				} else {
					dataTemp = defaultData
				}
				for (var key in dataTemp) {
					this[key] = dataTemp[key]
				}
			},

			selectPage(id) {
				if (this.loading || this.cards.length < 2) return

				if (this.cards.length < 2) {
					this.message = 'No se puede eliminar la única pagina'
					return
				}

				if (this.cards.length - this.numCardsSelected < 2) {
					this.message = 'no se pueden eliminar todas las paginas, por lo menos deja una.'
				}

				if (this.cards.length - this.numCardsSelected > 1 || this.cards[id].selected) {
					this.cards[id].selected = !this.cards[id].selected

					this.message = ''
				}
			},
			clearSelected() {
				for (var i = 0; i < this.cards.length; i++) {
					this.cards[i].selected = false
				}
			},
			onDeleted() {
				if (this.numCardsSelected === 0) {
					this.message = 'selecciona paginas para eliminar!'
					return
				}
				const loader2 = document.getElementById('loader2')

				loader2.style.height = document.body.scrollHeight + 120 + 'px'

				loader2.classList.remove('fade-out-loader')

				this.loading = true
				this.message = 'Eliminando pagina..'
				const st = setTimeout(() => {
					var payload = this.cards
						.reduce((res, card) => {
							if (!card.selected) {
								res.push(card.id + 1)
							}
							return res
						}, [])
						.join(',')
					fetch('/Index?handler=DeletePages&directory=' + this.file.dir + '&pagesString=' + payload)
						.then((dataRe) => {
							return dataRe.json()
						})
						.then((result) => {
							loader2.classList.add('fade-out-loader')
							if (result.status) {
								var index = 0
								this.cards = this.cards.reduce((res, card) => {
									if (!card.selected) {
										res.push({ ...card, id: index++ })
									}
									return res
								}, [])

								this.downloadable = true
								this.message = result.message
							}
							const to = setTimeout(() => {
								clearTimeout(to)
								this.loading = false
								const stime = setTimeout(() => {
									this.message = ''
									clearTimeout(stime)
								}, 4000)
							}, 500)
						})

					clearTimeout(st)
				}, 100)
			},
			onDownload() {
				swBeacom = false

				window.location = '/Index?handler=Download&dir=' + this.file.dir + '&pdfName=' + this.file.name

				swBeacom = true
			},
			reset() {
				if (controller) {
					controller.abort()
				}
				const to = setTimeout(() => {
					clearTimeout(to)
					location.reload()
				}, 500)
			},

			onClickSelectFile() {
				var inputFile = document.getElementById('inputFile')
				inputFile.click()
			},

			validateInputFile() {
				const response = { message: '', size: 0, name: '', isValid: false }

				if (!window.FileReader) {
					response.message = "The file API isn't supported on this browser yet."
					return response
				}

				var input = document.getElementById('inputFile')

				if (!input.files) {
					response.message = "This browser doesn't seem to support the `files` property of file inputs."
					return response
				} else if (!input.files[0]) {
					response.message = 'Selecciona un archivo pdf'
					return response
				} else if (input.files[0].size > limitSizeUpload) {
					response.message = 'Solo se permiten archivos menores a ' + limitSizeUpload / 1000000 + ' Mb.!!'
					return response
				} else if (input.files[0].type !== 'application/pdf') {
					response.message = 'Solo puedes elegir archivos PDF'
					return response
				} else {
					response.isValid = true
					response.name = input.files[0].name
					response.size = Math.round(input.files[0].size / 1000000) + ' Mb.' //a Mb's.
					return response
				}
			},

			onSubmit(evt) {
				evt.preventDefault()

				this.loading = true

				const respValidate = this.validateInputFile()

				if (!respValidate.isValid) {
					this.loading = false
					this.message = respValidate.message
					return
				}
				controller = new AbortController()
				this.message = 'Cargando ' + respValidate.name + ' (' + respValidate.size + ') ...ten paciencia'
				let data = new FormData(document.forms[0])
				fetch('/Index?handler=UploadFilePdf', {
					method: 'post',
					body: data,
					signal: controller.signal
				})
					.then((dataRe) => {
						return dataRe.json()
					})
					.then((res) => {
						this.loading = false
						if (res.status) {
							this.loading = true
							this.message =
								'Generando la vista para: ' +
								respValidate.name +
								' (' +
								res.payload.numPages +
								' paginas) ...ten paciencia'

							this.file.name = res.payload.name
							this.file.dir = res.payload.dir
							this.file.numPages = res.payload.numPages

							fetch('/Index?handler=MakeImages&nameDir=' + this.file.dir, { signal: controller.signal })
								.then((dataRe2) => {
									return dataRe2.json()
								})
								.then((res2) => {
									this.loading = false

									if (res2.status) {
										this.message = 'No olvides limpiar tu area de trabajo cuando termines'
										res2.payload.forEach((item) => {
											this.cards.push({ ...item, selected: false })
										})
									} else {
										this.message = res2.message
									}
								})
						} else {
							this.loading = false

							this.message = res.message
						}
					})
			},

			uploadFile() {
				document.getElementById('submit').click()
			},
			closeAlert(e) {
				this.message = ''
			},
			zoomPage(e, id) {
				e.preventDefault()
				e.stopPropagation()
				this.currentSelected = this.cards[id]
				this.modal = true
			},
			toggleSelectImage() {
				this.currentSelected.selected = !this.currentSelected.selected
			}
		},

		destroyed() {
			document.getElementById('submit').removeEventListener('click', this.onSubmit)
			window.removeEventListener('beforeunload', (e) => this.onUnloadHandler(e))
		}
	})
}
