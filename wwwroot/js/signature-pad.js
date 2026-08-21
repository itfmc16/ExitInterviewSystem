/**
 * Adobe Acrobat–style multi-mode signature capture.
 * Modes: Type | Draw | Upload image | Mobile (QR / open on phone)
 */
(function (window) {
    'use strict';

    function SignaturePad(canvas, hiddenInput, options) {
        options = options || {};
        this.canvas = canvas;
        this.input = hiddenInput;
        this.ctx = canvas.getContext('2d');
        this.drawing = false;
        this.hasInk = false;
        this.penColor = options.penColor || '#1a1a2e';
        this.penWidth = options.penWidth || 2.2;
        this._resize();
        this._bind();
        if (this.input && this.input.value && this.input.value.indexOf('data:image') === 0) {
            this._loadImage(this.input.value);
        }
    }

    SignaturePad.prototype._resize = function () {
        var ratio = Math.max(window.devicePixelRatio || 1, 1);
        var rect = this.canvas.getBoundingClientRect();
        var w = Math.max(rect.width || 500, 300);
        var h = Math.max(rect.height || 160, 120);
        this.canvas.width = w * ratio;
        this.canvas.height = h * ratio;
        this.ctx.setTransform(ratio, 0, 0, ratio, 0, 0);
        this.ctx.lineCap = 'round';
        this.ctx.lineJoin = 'round';
        this.ctx.strokeStyle = this.penColor;
        this.ctx.lineWidth = this.penWidth;
    };

    SignaturePad.prototype._pos = function (e) {
        var rect = this.canvas.getBoundingClientRect();
        var clientX, clientY;
        if (e.touches && e.touches.length) {
            clientX = e.touches[0].clientX;
            clientY = e.touches[0].clientY;
        } else {
            clientX = e.clientX;
            clientY = e.clientY;
        }
        return { x: clientX - rect.left, y: clientY - rect.top };
    };

    SignaturePad.prototype._start = function (e) {
        e.preventDefault();
        this.drawing = true;
        var p = this._pos(e);
        this.ctx.beginPath();
        this.ctx.moveTo(p.x, p.y);
    };

    SignaturePad.prototype._move = function (e) {
        if (!this.drawing) return;
        e.preventDefault();
        var p = this._pos(e);
        this.ctx.lineTo(p.x, p.y);
        this.ctx.stroke();
        this.hasInk = true;
    };

    SignaturePad.prototype._end = function (e) {
        if (!this.drawing) return;
        e.preventDefault();
        this.drawing = false;
        this._save();
    };

    SignaturePad.prototype._bind = function () {
        var self = this;
        this.canvas.addEventListener('mousedown', function (e) { self._start(e); });
        this.canvas.addEventListener('mousemove', function (e) { self._move(e); });
        this.canvas.addEventListener('mouseup', function (e) { self._end(e); });
        this.canvas.addEventListener('mouseleave', function (e) { self._end(e); });
        this.canvas.addEventListener('touchstart', function (e) { self._start(e); }, { passive: false });
        this.canvas.addEventListener('touchmove', function (e) { self._move(e); }, { passive: false });
        this.canvas.addEventListener('touchend', function (e) { self._end(e); });
        this.canvas.addEventListener('touchcancel', function (e) { self._end(e); });
        window.addEventListener('resize', function () {
            var data = self.hasInk ? self.canvas.toDataURL('image/png') : null;
            self._resize();
            if (data) self._loadImage(data);
        });
    };

    SignaturePad.prototype._save = function () {
        if (!this.input) return;
        if (this.hasInk) this.input.value = this.canvas.toDataURL('image/png');
    };

    SignaturePad.prototype.clear = function () {
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
        this.hasInk = false;
        if (this.input) this.input.value = '';
    };

    SignaturePad.prototype._loadImage = function (dataUrl, callback) {
        var self = this;
        var img = new Image();
        img.onload = function () {
            var w = self.canvas.getBoundingClientRect().width;
            var h = self.canvas.getBoundingClientRect().height;
            self.ctx.clearRect(0, 0, self.canvas.width, self.canvas.height);
            // Fit image into canvas preserving aspect ratio
            var scale = Math.min(w / img.width, h / img.height, 1);
            var dw = img.width * scale;
            var dh = img.height * scale;
            var dx = (w - dw) / 2;
            var dy = (h - dh) / 2;
            self.ctx.drawImage(img, dx, dy, dw, dh);
            self.hasInk = true;
            self._save();
            if (callback) callback();
        };
        img.src = dataUrl;
    };

    SignaturePad.prototype.renderTypedName = function (name, fontFamily) {
        this.clear();
        if (!name || !name.trim()) return;
        var w = this.canvas.getBoundingClientRect().width;
        var h = this.canvas.getBoundingClientRect().height;
        var font = fontFamily || "'Brush Script MT', 'Segoe Script', 'Lucida Handwriting', cursive";
        var size = Math.min(48, Math.max(28, Math.floor(w / (name.length * 0.55))));
        this.ctx.font = size + 'px ' + font;
        this.ctx.fillStyle = this.penColor;
        this.ctx.textAlign = 'center';
        this.ctx.textBaseline = 'middle';
        this.ctx.fillText(name.trim(), w / 2, h / 2 - 8);
        this.hasInk = true;
        this._save();
    };

    window.SignaturePad = SignaturePad;

    /* ---------- Multi-mode UI controller ---------- */
    window.initSignaturePad = function (canvasId, inputId) {
        var canvas = document.getElementById(canvasId);
        var input = document.getElementById(inputId);
        if (!canvas) return null;

        var pad = new SignaturePad(canvas, input);
        var root = canvas.closest('.sig-adobe') || document;

        function showPanel(mode) {
            root.querySelectorAll('.sig-panel').forEach(function (el) {
                el.classList.toggle('active', el.getAttribute('data-mode') === mode);
            });
            root.querySelectorAll('.sig-mode-btn').forEach(function (btn) {
                btn.classList.toggle('active', btn.getAttribute('data-mode') === mode);
            });
            // Redraw canvas size when switching to draw
            if (mode === 'draw') {
                setTimeout(function () {
                    var data = pad.hasInk && input.value ? input.value : null;
                    pad._resize();
                    if (data && data.indexOf('data:image') === 0) pad._loadImage(data);
                }, 50);
            }
        }

        // Mode buttons
        root.querySelectorAll('.sig-mode-btn').forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                showPanel(btn.getAttribute('data-mode'));
            });
        });

        // Clear
        var clearBtn = root.querySelector('[data-sig-clear]');
        if (clearBtn) {
            clearBtn.addEventListener('click', function (e) {
                e.preventDefault();
                pad.clear();
                var typeInput = root.querySelector('.sig-type-input');
                if (typeInput) typeInput.value = '';
                var fileInput = root.querySelector('.sig-upload-input');
                if (fileInput) fileInput.value = '';
                var preview = root.querySelector('.sig-upload-preview');
                if (preview) { preview.style.display = 'none'; preview.removeAttribute('src'); }
            });
        }

        // TYPE mode
        var typeInput = root.querySelector('.sig-type-input');
        var typeFont = root.querySelector('.sig-type-font');
        function applyTyped() {
            if (!typeInput) return;
            pad.renderTypedName(typeInput.value, typeFont ? typeFont.value : null);
        }
        if (typeInput) {
            typeInput.addEventListener('input', applyTyped);
            typeInput.addEventListener('change', applyTyped);
        }
        if (typeFont) typeFont.addEventListener('change', applyTyped);
        var typeApply = root.querySelector('.sig-type-apply');
        if (typeApply) typeApply.addEventListener('click', function (e) { e.preventDefault(); applyTyped(); });

        // UPLOAD mode
        var fileInput = root.querySelector('.sig-upload-input');
        if (fileInput) {
            fileInput.addEventListener('change', function () {
                var file = fileInput.files && fileInput.files[0];
                if (!file) return;
                if (!file.type.match(/^image\//)) {
                    alert('Please choose an image file (PNG, JPG, GIF, or WebP).');
                    return;
                }
                if (file.size > 2 * 1024 * 1024) {
                    alert('Image is too large. Please use a file under 2 MB.');
                    return;
                }
                var reader = new FileReader();
                reader.onload = function (ev) {
                    var dataUrl = ev.target.result;
                    pad._loadImage(dataUrl);
                    var preview = root.querySelector('.sig-upload-preview');
                    if (preview) {
                        preview.src = dataUrl;
                        preview.style.display = 'block';
                    }
                    // Switch to draw so user can see result on pad
                    showPanel('draw');
                };
                reader.readAsDataURL(file);
            });
        }

        // MOBILE mode – open sign URL / copy link
        var mobileLink = root.querySelector('.sig-mobile-link');
        var mobileCopy = root.querySelector('.sig-mobile-copy');
        var mobileOpen = root.querySelector('.sig-mobile-open');
        function mobileUrl() {
            // Same page with hash so phone can open and use Draw tab
            var u = new URL(window.location.href);
            u.searchParams.set('sigMode', 'draw');
            u.hash = 'employeeSignatureCanvas';
            return u.toString();
        }
        if (mobileLink) mobileLink.value = mobileUrl();
        if (mobileCopy) {
            mobileCopy.addEventListener('click', function (e) {
                e.preventDefault();
                var url = mobileUrl();
                if (mobileLink) mobileLink.value = url;
                if (navigator.clipboard && navigator.clipboard.writeText) {
                    navigator.clipboard.writeText(url).then(function () {
                        mobileCopy.textContent = 'Copied!';
                        setTimeout(function () { mobileCopy.textContent = 'Copy link'; }, 2000);
                    });
                } else if (mobileLink) {
                    mobileLink.select();
                    document.execCommand('copy');
                    mobileCopy.textContent = 'Copied!';
                    setTimeout(function () { mobileCopy.textContent = 'Copy link'; }, 2000);
                }
            });
        }
        if (mobileOpen) {
            mobileOpen.addEventListener('click', function (e) {
                e.preventDefault();
                window.open(mobileUrl(), '_blank');
            });
        }

        // Default panel
        var initial = (new URLSearchParams(window.location.search)).get('sigMode') || 'draw';
        showPanel(initial);

        // If existing typed (non-image) value, show in type field
        if (input && input.value && input.value.indexOf('data:image') !== 0 && typeInput) {
            typeInput.value = input.value;
            showPanel('type');
            applyTyped();
        }

        return pad;
    };
})(window);
