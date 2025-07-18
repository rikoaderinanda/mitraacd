
function toggleBottomSheet(Id) {
    document.getElementById(Id).classList.toggle("show");
}

const Storage = {
    set(key, value) {
        localStorage.setItem(key, JSON.stringify(value));
    },
    get(key) {
        const item = localStorage.getItem(key);
        return item ? JSON.parse(item) : null;
    },
    remove(key) {
        localStorage.removeItem(key);
    },
    clear() {
        localStorage.clear();
    }
};

function initCarousel(Element, Qty) {
    var $slider = $('#' + Element + '');
    if ($slider.hasClass('owl-loaded')) {
        $slider.trigger('destroy.owl.carousel');
    }
    $slider.owlCarousel({
        loop: $slider.find('.item').length > 10,
        margin: 16,
        nav: false,
        dots: true,
        responsive: {
            0: { items: Qty },
            600: { items: Qty },
            1000: { items: Qty }
        }
    });
}

function lazyAnimateItems(Element) {
    var items = document.querySelectorAll('#' + Element + ' .fade-in');
    if ('IntersectionObserver' in window) {
        var observer = new IntersectionObserver(function (entries, obs) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('visible');
                    obs.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1 });
        items.forEach(function (item) { observer.observe(item); });
    } else {
        // fallback
        items.forEach(function (item) { item.classList.add('visible'); });
    }
}

function formatRupiah(angka) {
    if (typeof angka !== 'number' || isNaN(angka)) return 'Rp 0';
    return 'Rp ' + angka.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
}

function validasiForm() {
    let isValid = true;

    // Reset error
    $(':input').removeClass('is-invalid');

    // Cek tiap input required
    $('input[required], textarea[required]').each(function () {
        let val = $(this).val().trim();
        let id = $(this).attr('id');
        // Validasi khusus untuk WhatsApp
        if (id === 'whatsapp') {
            if (!isValidWhatsapp(val)) {
                $(this).addClass('is-invalid');
                $(this)
                    .next('.invalid-feedback')
                    .text('Format nomor WhatsApp tidak valid.');
                isValid = false;
                return;
            } else {
                $(this).removeClass('is-invalid');
            }
        }

        if (!val) {
            $(this).addClass('is-invalid'); // Tambah class untuk trigger pesan error
            isValid = false;
        } else {
            if (id !== 'whatsapp') {
                $(this).removeClass('is-invalid');
            }
        }
    });

    return isValid;
}

function isValidWhatsapp(number) {
    let cleaned = number.replace(/\D/g, ''); // Hapus semua karakter non-digit
    return /^((62)|0)8[1-9][0-9]{7,11}$/.test(cleaned); // Validasi pola umum WhatsApp di Indonesia
}

function getEmptyFields(obj, path = '') {
    let emptyFields = [];

    for (const key in obj) {
        if (obj.hasOwnProperty(key)) {
            const val = obj[key];
            const currentPath = path ? `${path}.${key}` : key;

            if (Array.isArray(val)) {
                if (val.length === 0) {
                    emptyFields.push(currentPath);
                } else {
                    // Jika array isinya object, iterasi setiap item
                    val.forEach((item, index) => {
                        if (typeof item === 'object' && item !== null) {
                            emptyFields.push(
                                ...getEmptyFields(
                                    item,
                                    `${currentPath}[${index}]`
                                )
                            );
                        }
                    });
                }
            } else if (typeof val === 'object' && val !== null) {
                emptyFields.push(...getEmptyFields(val, currentPath));
            } else if (val === '' || val === null || val === undefined) {
                emptyFields.push(currentPath);
            }
        }
    }

    return emptyFields;
}

function postApi(options) {
    const {
        url,
        data,
        onSuccess = function () {},
        onError = function () {},
        onBeforeSend = function () {},
        onComplete = function () {}
    } = options;

    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        beforeSend: function () {
            console.log('Mengirim request ke:', url);
            onBeforeSend();
        },
        success: function (response) {
            console.log('Respon sukses:', response);
            onSuccess(response);
        },
        error: function (xhr, status, error) {
            let message = 'Terjadi kesalahan saat mengirim data.';

            if (xhr.responseJSON && xhr.responseJSON.message) {
                message = xhr.responseJSON.message;
            } else if (xhr.status === 0) {
                message = 'Tidak dapat terhubung ke server.';
            } else if (xhr.status >= 400) {
                message = `Error ${xhr.status}: ${xhr.statusText}`;
            }

            console.error('Error:', message);
            onError(xhr, message);
        },
        complete: function () {
            onComplete();
        }
    });
}

function getStoragePemesanan() {
    var keranjang = Storage.get('keranjang') || [];
    var JenisProperti = Storage.get('jenis_properti') || [];
    var Keluhan = Storage.get('keluhan') || [];
    var TanggalKunjungan = Storage.get('TanggalKunjungan') || '';
    var JamKunjungan = Storage.get('JamKunjungan') || '';
    var Total = Storage.get('Total') || 0;
    var Customer = Storage.get('Customer') || [];
    var IdPesanan = Storage.get('IdPesanan') || 0;
    var id_layanan = Storage.get('id_layanan') || 0;
    var user_id = Storage.get('userId') || null;
    var data = {
        Keranjang: keranjang,
        JenisProperti: JenisProperti,
        Keluhan: Keluhan,
        TanggalKunjungan: TanggalKunjungan,
        JamKunjungan: JamKunjungan,
        Total: Total,
        Customer: Customer,
        Id: IdPesanan,
        id_layanan: id_layanan,
        user_id: user_id
    };

    return data;
}

function callApi(options) {
    const {
        url,
        method = 'GET',
        data = null,
        token = null,
        contentType = 'application/json',
        success = function () {},
        error = function () {},
        onBeforeSend = function () {},
        onComplete = function () {}
    } = options;

    $.ajax({
        url: url,
        method: method,
        contentType: contentType,
        data:
            contentType === 'application/json' && data
                ? JSON.stringify(data)
                : data,
        headers: token ? { Authorization: `Bearer ${token}` } : {},
        success: function (response) {
            success(response);
        },
        error: function (xhr) {
            const errMsg =
                xhr.responseJSON?.message || xhr.statusText || 'API Error';
            console.error('API Error:', errMsg);
            error(errMsg);
        },
        beforeSend: function () {
            // console.log('Mengirim request ke:', url);
            onBeforeSend();
        },
        complete: function () {
            onComplete();
        }
    });
}

function formatToSixDigits(number) {
    return number.toString().padStart(6, '0');
}

function isValidUser(u) {
    return (
        u != null &&
        typeof u === 'object' &&
        u.id != null &&
        typeof u.username === 'string' &&
        u.username.trim() !== ''
    );
}

function getUserInfo() {
    var data = {
        id: Storage.get('userId') || '',
        username: Storage.get('username') || '',
        nama_lengkap: Storage.get('nama_lengkap') || '',
        email: Storage.get('email') || ''
    };
    return data;
}

function getSalamWaktu() {
    const jam = new Date().getHours();

    if (jam >= 4 && jam < 11) {
        return 'Pagi';
    } else if (jam >= 11 && jam < 15) {
        return 'Siang';
    } else if (jam >= 15 && jam < 18) {
        return 'Sore';
    } else {
        return 'Malam';
    }
}