
function toggleBottomSheet(Id) {
    document.getElementById(Id).classList.toggle('show');
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
        var observer = new IntersectionObserver(
            function (entries, obs) {
                entries.forEach(function (entry) {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('visible');
                        obs.unobserve(entry.target);
                    }
                });
            },
            { threshold: 0.1 }
        );
        items.forEach(function (item) {
            observer.observe(item);
        });
    } else {
        // fallback
        items.forEach(function (item) {
            item.classList.add('visible');
        });
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

function error_getLocation(err) {
    console.error('Error:', err.message);
}

async function getAddress_api(lat, lng) {
    return new Promise((resolve, reject) => {
        callApi({
            url: `/api/Location/reverse-geocode?lat=${lat}&lng=${lng}`,
            method: 'GET',
            success: function (res) {
                resolve(res); // kembalikan data API
            },
            error: function () {
                console.log('Proses gagal.');
                //Swal.fire('Gagal!', 'Proses gagal.', 'warning');
                reject('API Error');
            },
            onBeforeSend: function () {
                // btn.html(
                //     `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`
                // );
                // btn.prop('disabled', true);
            },
            onComplete: function () {
                // btn.html(
                //     `<i class="bi bi-arrow-right" style="font-size: 1rem;"></i>`
                // );
                // btn.prop('disabled', false);
            }
        });
    });
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

    const isFormData = data instanceof FormData;

    $.ajax({
        url: url,
        method: method,
        data: isFormData
            ? data
            : contentType === 'application/json' && data
            ? JSON.stringify(data)
            : data,
        contentType: isFormData ? false : contentType,
        processData: isFormData ? false : true,
        headers: token ? { Authorization: `Bearer ${token}` } : {},
        beforeSend: function () {
            onBeforeSend();
        },
        success: function (response) {
            success(response);
        },
        error: function (xhr) {
            const errMsg =
                xhr.responseJSON?.message || xhr.statusText || 'API Error';
            console.log('Error:', errMsg, 'Status:', xhr.status);
            showToast('Error:' + errMsg);

            // ✅ Munculkan modal hanya jika koneksi terputus
            if (xhr.status === 0) {
                $('#ModalConnectionError').modal('show');
            } else {
                // kalau mau bisa tampilkan toast / swal error biasa
                showToast(errMsg);
            }
        },
        complete: function () {
            onComplete();
        }
    });
}

function formatToSixDigits(number) {
    return number.toString().padStart(6, '0');
}

function getUserInfo() {
    var data = {
        id: Storage.get('userId') || '',
        username: Storage.get('username') || '',
        nama_lengkap: Storage.get('nama_lengkap') || '',
        email: Storage.get('email') || '',
        no_hp: Storage.get('no_hp') || '',
        photo: Storage.get('photo') || ''
    };
    return data;
}

function clearSessionForLOgout() {
    Storage.remove('userId');
    Storage.remove('username');
    Storage.remove('email');
    Storage.remove('nama_lengkap');
    Storage.remove('no_hp');
    Storage.remove('photo');
    Storage.remove('jwt');
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

function getMitraInfo() {
    var data = {
        id: Storage.get('userId') || '',
        username: Storage.get('username') || '',
        nama_lengkap: Storage.get('nama_lengkap') || '',
        email: Storage.get('email') || '',
        no_hp: Storage.get('no_hp') || '',
        photo: Storage.get('photo') || ''
    };
    return data;
}

function dataURLToBlob(dataURL) {
    const [meta, content] = dataURL.split(',');
    const mime = meta.match(/:(.*?);/)[1];
    const binary = atob(content);
    const array = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        array[i] = binary.charCodeAt(i);
    }
    return new Blob([array], { type: mime });
}

function getCleanToken() {
    let token = localStorage.getItem('jwt');
    if (!token) return null;

    // hapus tanda kutip di depan & belakang kalau ada
    if (token.startsWith('"') && token.endsWith('"')) {
        token = token.substring(1, token.length - 1);
    }

    return token;
}

function decodeJwt(token) {
    const parts = token.split('.');
    if (parts.length !== 3) return null;

    const payload = JSON.parse(atob(parts[1]));
    console.log('Header:', JSON.parse(atob(parts[0])));
    console.log('Payload:', payload);
    return payload;
}

async function GetDataAkunApi(id) {
    return new Promise((resolve, reject) => {
        callApi({
            url: '/api/Auth/GetData_account?id=' + id,
            method: 'GET',
            success: function (res) {
                resolve(res);
            },
            error: function (err) {
                reject(err);
            },
            onBeforeSend: function () {},
            onComplete: function () {}
        });
    });
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

async function checkStatusMitra() {
    var user = getUserInfo();

    if (!isValidUser(user)) {
        $('#TitleModalLogin').text('Silahkan login terlebih dahulu');
        $('#btnClsModalLogin').show();
        $('#ModalLogin').modal('show');

        return { success: false, reason: 'not_logged_in' };
    } else {
        try {
            var data = await GetDataAkunApi(user.id);
            return { data: data };
            // console.log(data);
            // if (data.status_mitra == 0) {
            //     $('#ModalRegistrasi').modal('show');
            //     return { success: false, reason: 'not_registered', data: data };
            // } else if (data.status_mitra == 1) {
            //     return { success: false, reason: 'not_registered', data: data };
            // }

            // return { success: true, reason: 'registered', data: data };
        } catch (err) {
            console.error('Error fetching akun:', err);
            return { success: false, reason: 'api_error', error: err };
        }
    }
}

async function cekNamaApi(_data) {
    return new Promise((resolve, reject) => {
        callApi({
            url: '/api/Auth/CheckNamaLengkapDanPanggilan',
            method: 'POST',
            data: _data,
            success: function (res) {
                resolve(res);
            },
            error: function (err) {
                reject(err);
            },
            onBeforeSend: function () {},
            onComplete: function () {}
        });
    });
}

async function getAddress_api(lat, lng) {
    return new Promise((resolve, reject) => {
        callApi({
            url: `/api/Location/reverse-geocode?lat=${lat}&lng=${lng}`,
            method: 'GET',
            success: function (res) {
                resolve(res); // kembalikan data API
            },
            error: function () {
                console.log('Proses gagal.');
                reject('API Error');
            },
            onBeforeSend: function () {},
            onComplete: function () {}
        });
    });
}

function base64ToBlob(base64Data) {
    const parts = base64Data.split(',');
    const mime = parts[0].match(/:(.*?);/)[1];
    const byteString = atob(parts[1]);
    const ab = new ArrayBuffer(byteString.length);
    const ia = new Uint8Array(ab);
    for (let i = 0; i < byteString.length; i++) {
        ia[i] = byteString.charCodeAt(i);
    }
    return new Blob([ab], { type: mime });
}

function formatDate(dateStr) {
    if (!dateStr) return '';

    // Pecah string dd-MM-yyyy
    const [day, month, year] = dateStr.split('-');

    // Nama bulan dalam bahasa Indonesia
    const monthNames = [
        'Januari',
        'Februari',
        'Maret',
        'April',
        'Mei',
        'Juni',
        'Juli',
        'Agustus',
        'September',
        'Oktober',
        'November',
        'Desember'
    ];

    // Validasi sederhana
    if (!day || !month || !year) return dateStr;

    return `${parseInt(day)} ${monthNames[parseInt(month) - 1]} ${year}`;
}
function formatTanggal(tanggalStr, plusHari = 0) {
    const date = new Date(tanggalStr);

    // Tambah hari kalau perlu
    if (plusHari !== 0) {
        date.setDate(date.getDate() + plusHari);
    }

    // Format ke Indonesia
    return date.toLocaleDateString('id-ID', {
        weekday: 'long', // Senin, Selasa, ...
        day: '2-digit',
        month: 'short', // Jan, Feb, Mar ...
        year: 'numeric'
    });
}

function formatPhoneNumber(phone) {
    // pastikan input string
    if (!phone) return '';

    // hilangkan karakter non-angka
    let digits = phone.replace(/\D/g, '');

    // kalau mulai dengan 62 → tambah tanda +
    if (digits.startsWith('62')) {
        digits = '+' + digits;
    }

    // contoh: +6281234567890
    // ambil prefix +62
    const prefix = digits.substring(0, 3); // +62
    const part1 = digits.substring(3, 6); // 812
    const part2 = digits.substring(6, 10); // 3456
    const part3 = digits.substring(10); // 7890

    return `${prefix} ${part1}-${part2}-${part3}`;
}

function showMaps(koordinat) {
    console.log(koordinat);
    if (!koordinat) {
        Swal.fire({
            icon: 'warning',
            title: 'Koordinat tidak tersedia',
            text: 'Alamat ini belum memiliki titik lokasi di Maps.'
        });
        return;
    }

    // pastikan format koordinat: "lat,lng"
    const [lat, lng] = koordinat.split(',');
    const mapUrl = `https://www.google.com/maps?q=${lat},${lng}`;

    // buka di tab baru
    window.open(mapUrl, '_blank');
}

function hitungJarakJalan(origin, destination) {
    const service = new google.maps.DirectionsService();

    return new Promise((resolve, reject) => {
        service.route(
            {
                origin: origin, // { lat: -6.5973, lng: 106.7660 }
                destination: destination, // { lat: -6.5930, lng: 106.7810 }
                travelMode: google.maps.TravelMode.DRIVING // atau WALKING, BICYCLING
            },
            (response, status) => {
                if (status === google.maps.DirectionsStatus.OK) {
                    const distance = response.routes[0].legs[0].distance.text; // e.g. "5.4 km"
                    const duration = response.routes[0].legs[0].duration.text; // e.g. "15 menit"
                    resolve({ distance, duration });
                } else {
                    reject('Gagal hitung jarak: ' + status);
                }
            }
        );
    });
}

function startCountdown(id, paramB, elementId) {
    const now = new Date();

    // format ke YYYY-MM-DD (local time)
    const today = [
        now.getFullYear(),
        String(now.getMonth() + 1).padStart(2, '0'),
        String(now.getDate()).padStart(2, '0')
    ].join('-');

    //const today = new Date().toISOString().split('T')[0];
    const endTime = new Date(`${today}T${paramB}:00`); // waktu target
    let diff = endTime - new Date(); // selisih dari sekarang

    if (diff <= 0) {
        document.getElementById(elementId).innerText = 'Waktu habis';
        $('#SLA_' + id).removeClass('d-none');
        return;
    }

    const timer = setInterval(() => {
        const now = new Date();
        diff = endTime - now;

        if (diff <= 0) {
            $('#SLA_' + id).removeClass('d-none');
            clearInterval(timer);
            document.getElementById(elementId).innerText = 'Waktu habis';
            return;
        }

        // Hitung jam, menit, detik
        const hours = Math.floor(diff / (1000 * 60 * 60));
        const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((diff % (1000 * 60)) / 1000);
        let durasiText = $('#durasi_' + id).text();
        let durasiMenit = parseDurationToMinutes(durasiText);
        if (minutes <= durasiMenit) {
            $('#SLA_' + id).removeClass(`d-none`);
        } else {
            $('#SLA_' + id).addClass(`d-none`);
        }
        // Update tampilan
        const el = document.getElementById(elementId);
        if (!el) {
            console.warn('Element countdown tidak ditemukan:', elementId);
            clearInterval(timer);
            return;
        }
        document.getElementById(elementId).innerText =
            `${hours.toString().padStart(2, '0')}:` +
            `${minutes.toString().padStart(2, '0')}:` +
            `${seconds.toString().padStart(2, '0')}`;
    }, 1000);
}

function parseDurationToMinutes(durationText) {
    // contoh input: "18 mins", "1 hour 5 mins", "45 min"
    let totalMinutes = 0;

    // Ambil jam kalau ada
    let hourMatch = durationText.match(/(\d+)\s*hour/);
    if (hourMatch) {
        totalMinutes += parseInt(hourMatch[1], 10) * 60;
    }

    // Ambil menit kalau ada
    let minuteMatch = durationText.match(/(\d+)\s*min/);
    if (minuteMatch) {
        totalMinutes += parseInt(minuteMatch[1], 10);
    }

    return totalMinutes;
}

function onGoogleScriptLoad() {
    console.log('✅ Google Identity Services script berhasil dimuat');
    // di sini kamu bisa inisialisasi tombol login Google
    google.accounts.id.initialize({
        client_id: 'YOUR_CLIENT_ID.apps.googleusercontent.com',
        callback: handleCredentialResponse
    });

    google.accounts.id.renderButton(document.getElementById('googleLoginDiv'), {
        theme: 'outline',
        size: 'large'
    });
}

function onGoogleScriptError() {
    console.error('❌ Gagal memuat Google Identity Services script');
}

function handleCredentialResponse(response) {
    console.log('ID Token:', response.credential);
    // kirim token ke server untuk verifikasi
}

function callUpdateStatusPadaPelanggan(_id_pelanggan,_Pesan){
    callApi({
        url: BASE_API_URL+'/api/Notifikasi/StatusTrackingOrder',
        method: 'POST',
        data: { userId : _id_pelanggan, pesan : _Pesan },
        success: function (res) {
        },
        error: function (err) {
            showToast("Gagal: " + err);
        },
        onBeforeSend: function () {
            
        },
        onComplete: function () {
            
        }
    });
}