// Helper untuk manage subscription event notifikasi
window.NotificationHelper = (function () {
    const activeHandlers = [];

    function subscribe(eventName, handler) {
        // register handler
        window.addEventListener(eventName, handler);
        activeHandlers.push({ eventName, handler });

        // auto-unsubscribe saat page unload
        window.addEventListener('beforeunload', () => {
            unsubscribe(eventName, handler);
        });
    }

    function unsubscribe(eventName, handler) {
        window.removeEventListener(eventName, handler);

        // bersihkan dari activeHandlers
        const index = activeHandlers.findIndex(
            (h) => h.eventName === eventName && h.handler === handler
        );
        if (index > -1) {
            activeHandlers.splice(index, 1);
        }
    }

    function clearAll() {
        activeHandlers.forEach(({ eventName, handler }) => {
            window.removeEventListener(eventName, handler);
        });
        activeHandlers.length = 0;
    }

    return {
        subscribe,
        unsubscribe,
        clearAll
    };
})();

window.SignalRHelper = (function () {
    let connection = null;

    async function init(token) {
        if (connection) return connection; // sudah ada, jangan buat lagi

        connection = new signalR.HubConnectionBuilder()
            .withUrl('/notifikasiHub', {
                accessTokenFactory: () => token // JWT dari login
            })
            .withAutomaticReconnect()
            .build();

        try {
            await connection.start();
            console.log('✅ SignalR connected:', connection.connectionId);
        } catch (err) {
            console.error('❌ Gagal connect SignalR:', err);
        }

        return connection;
    }

    function addHandler(eventName, handler) {
        if (!connection) {
            console.warn('⚠️ SignalR belum diinit, panggil init(token) dulu.');
            return;
        }

        connection.on(eventName, (pesan) => {
            console.log(`📩 Event ${eventName} diterima:`, pesan);

            // propagate ke NotificationHelper
            window.dispatchEvent(new CustomEvent(eventName, { detail: pesan }));

            // jalankan handler tambahan kalau ada
            if (typeof handler === 'function') {
                handler(pesan);
            }
        });
    }

    function getConnection() {
        return connection;
    }

    return {
        init,
        addHandler,
        getConnection
    };
})();
