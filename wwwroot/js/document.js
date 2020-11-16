// window.addEventListener('load', () => {
// 	const textDownload = 'modificado'
// 	var vm = new Vue({
// 		el: '#app',
// 		data: {
// 			cards: [],
// 			loading: false,
// 			// subTiTle:'Carga un documento pdf',
// 			btnText: 'Eliminar Seleccionadas',
// 			msgError: '',
// 			file: {
// 				name: 'LABORATORIOS JUAN CARLOS SALAUES 22.10.2020 v23',
// 				size: '1Mb'
// 			}
// 		},
// 		mounted: function() {
// 			// for (let i = 0; i < 8; i++) {
// 			// 	this.cards.push({ id: i, selected: false })
// 			// }

// 			fetch('/Document?handler=PagesPdf').then((response) => console.log(response))

// 			// console.log('cards', this.cards)
// 		},
// 		computed: {
// 			numCardsSelected: function() {
// 				return this.cards.filter(function(card) {
// 					return card.selected
// 				}).length
// 			},
// 			downloadable: function() {
// 				return this.file.name.indexOf(textDownload) > -1
// 			},
// 			subTiTle: function() {
// 				return this.cards.length > 0 ? 'Selecciona las paginas que deseas eliminar' : 'Carga un documento pdf'
// 			}
// 		},
// 		methods: {
// 			selectPage: function(id) {
// 				if (this.loading || this.cards.length < 2) return

// 				if (this.cards.length < 2) {
// 					this.msgError = 'No se puede eliminar la única pagina'
// 					return
// 				}

// 				if (this.cards.length - this.numCardsSelected < 2) {
// 					this.msgError = 'no se pueden eliminar todas las paginad, por lo menos deja una.'
// 				}

// 				const index = this.cards.findIndex((c) => c.id === id)
// 				if (this.cards.length - this.numCardsSelected > 1 || this.cards[index].selected) {
// 					this.cards[index].selected = !this.cards[index].selected
// 				}
// 			},
// 			clearSelected: function() {
// 				for (var i = 0; i < this.cards.length; i++) {
// 					this.cards[i].selected = false
// 				}
// 			},
// 			onDeleted: function() {
// 				this.loading = true
// 				setTimeout(() => {
// 					this.cards = this.cards.filter((card) => !card.selected)
// 					this.loading = false
// 					if (!this.downloadable) {
// 						this.file.name += '-' + textDownload
// 					}
// 				}, 1000)
// 			},
// 			reset: function() {
// 				this.cards = []
// 				this.file = { name: '', size: '' }
// 				this.msgError = ''
// 				this.loading = false
// 			}
// 		}
// 	})
// })
