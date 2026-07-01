window.boltonCupDownload = {
    downloadUrl: function (url, filename) {
        const a = document.createElement('a');
        a.href = url;
        a.download = filename || '';
        a.target = '_blank';
        a.rel = 'noopener';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    }
};
