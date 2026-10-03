window.studyApp = {
    // Play pleasant synthesized chime using Web Audio API (no external mp3 file needed)
    playChime: function () {
        try {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (!AudioContext) return;
            const ctx = new AudioContext();

            // Two-tone cheerful chime
            const playNote = (freq, startTime, duration) => {
                const osc = ctx.createOscillator();
                const gain = ctx.createGain();
                osc.type = 'sine';
                osc.frequency.setValueAtTime(freq, startTime);

                gain.gain.setValueAtTime(0, startTime);
                gain.gain.linearRampToValueAtTime(0.2, startTime + 0.05);
                gain.gain.exponentialRampToValueAtTime(0.001, startTime + duration);

                osc.connect(gain);
                gain.connect(ctx.destination);

                osc.start(startTime);
                osc.stop(startTime + duration);
            };

            const now = ctx.currentTime;
            playNote(523.25, now, 0.4);        // C5
            playNote(659.25, now + 0.15, 0.4); // E5
            playNote(783.99, now + 0.3, 0.6);  // G5
            playNote(1046.50, now + 0.45, 0.8);// C6
        } catch (e) {
            console.warn('Audio chime error:', e);
        }
    },

    setTitle: function (title) {
        document.title = title;
    },

    setTheme: function (theme) {
        document.documentElement.setAttribute('data-theme', theme);
        if (theme === 'dark') {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
        localStorage.setItem('study_theme', theme);
    },

    getInitialTheme: function () {
        const saved = localStorage.getItem('study_theme');
        if (saved) return saved;
        return window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark';
    }
};

// Initialize theme on script load
(function() {
    const theme = window.studyApp.getInitialTheme();
    document.documentElement.setAttribute('data-theme', theme);
    if (theme === 'dark') {
        document.documentElement.classList.add('dark');
    } else {
        document.documentElement.classList.remove('dark');
    }
})();
