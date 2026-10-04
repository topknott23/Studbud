window.studyApp = {
    _alarmAudio: null,
    _synthAlarmInterval: null,

    // Start repeating/looping alarm audio
    startAlarm: function (soundPath) {
        this.stopAlarm();
        const path = soundPath || 'alarm/Chinese Meme Ringtone Download.mp3';
        
        try {
            // Encode the path to properly handle spaces and special characters
            const encodedPath = encodeURI(path);
            const audio = new Audio(encodedPath);
            audio.loop = true;
            audio.volume = 1.0;
            this._alarmAudio = audio;

            const playPromise = audio.play();
            if (playPromise !== undefined) {
                playPromise.catch((error) => {
                    console.warn('HTML5 Audio playback prevented/failed, falling back to Web Audio alarm:', error);
                    this.startSynthAlarm();
                });
            }
        } catch (err) {
            console.warn('Audio initialization error, using synth alarm fallback:', err);
            this.startSynthAlarm();
        }

        // Haptic feedback vibration on mobile devices
        if (navigator.vibrate) {
            try {
                navigator.vibrate([400, 200, 400, 200, 600]);
            } catch (e) { }
        }
    },

    // Stop and reset the alarm sound
    stopAlarm: function () {
        if (this._alarmAudio) {
            try {
                this._alarmAudio.pause();
                this._alarmAudio.currentTime = 0;
            } catch (e) { }
            this._alarmAudio = null;
        }
        this.stopSynthAlarm();
    },

    // Check if alarm is currently ringing
    isAlarmPlaying: function () {
        return (this._alarmAudio !== null && !this._alarmAudio.paused) || (this._synthAlarmInterval !== null);
    },

    // Web Audio repeating alarm fallback
    startSynthAlarm: function () {
        this.stopSynthAlarm();
        this.playSynthBeepPattern();
        this._synthAlarmInterval = setInterval(() => {
            this.playSynthBeepPattern();
        }, 1200);
    },

    stopSynthAlarm: function () {
        if (this._synthAlarmInterval) {
            clearInterval(this._synthAlarmInterval);
            this._synthAlarmInterval = null;
        }
    },

    playSynthBeepPattern: function () {
        try {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (!AudioContext) return;
            const ctx = new AudioContext();
            const now = ctx.currentTime;
            
            // Triple upbeat beep sequence
            const beeps = [880, 880, 1174.66]; // A5, A5, D6
            beeps.forEach((freq, idx) => {
                const osc = ctx.createOscillator();
                const gain = ctx.createGain();
                osc.type = 'triangle';
                osc.frequency.setValueAtTime(freq, now + (idx * 0.14));

                gain.gain.setValueAtTime(0, now + (idx * 0.14));
                gain.gain.linearRampToValueAtTime(0.25, now + (idx * 0.14) + 0.02);
                gain.gain.exponentialRampToValueAtTime(0.001, now + (idx * 0.14) + 0.12);

                osc.connect(gain);
                gain.connect(ctx.destination);

                osc.start(now + (idx * 0.14));
                osc.stop(now + (idx * 0.14) + 0.12);
            });
        } catch (e) { }
    },

    // Play pleasant synthesized chime for soft notification
    playChime: function () {
        try {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (!AudioContext) return;
            const ctx = new AudioContext();

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
