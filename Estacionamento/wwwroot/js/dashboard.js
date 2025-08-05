document.addEventListener("DOMContentLoaded", function () {
    const ctx = document.getElementById('graficoReceita').getContext('2d');

    const chart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: window.dashboardLabels,
            datasets: [{
                label: 'Receita (R$)',
                data: window.dashboardValores,
                backgroundColor: '#00d1b2',
                borderColor: '#00b28f',
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
});
