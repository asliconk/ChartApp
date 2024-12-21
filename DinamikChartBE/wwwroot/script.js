$(document).ready(() => {
    let myChart;


    const makeAjaxCall = (url, method, data = null, successCallback, errorCallback) => {
        $.ajax({
            url,
            method,
            contentType: "application/json",
            data: data ? JSON.stringify(data) : undefined,
            success: successCallback,
            error: errorCallback
        });
    };

    $("#dbForm").on("submit", (event) => {
        event.preventDefault();

        const connectionData = {
            server: $("#server").val(),
            username: $("#user").val(),
            password: $("#pass").val(),
            databaseName: $("#db").val()
        };

        const connectionMessage = $("#connectionMessage");

        makeAjaxCall(
            "https://localhost:7031/api/Database/validateConnection",
            "POST",
            connectionData,
            (data) => {
                if (data.status) {
                    connectionMessage.text("Baglanildi.").show();
                    $("#columnsSection").show();
                    setTimeout(() => connectionMessage.fadeOut(), 3000);
                    fetchProcedures();
                    fetchViews();
                } else {
                    connectionMessage.text("Bilgilerinizde bir hata var, l�tfen d�zeltin.").show();
                }
            },
            (error) => {
                connectionMessage.text("Hata: " + error.message).css("color", "red").show();
            }
        );
    });

    const fetchProcedures = () => {
        makeAjaxCall(
            "https://localhost:7031/api/Database/fetchStoredProcedures",
            "GET",
            null,
            (procedures) => {
                const comboBox = $("#proceduresComboBox");
                comboBox.empty();
                procedures.forEach((procedure) => {
                    comboBox.append(new Option(procedure, procedure));
                });
                $("#proceduresComboBox, #executeProcedureButton").show();
            },
            (error) => console.error("Prosed�rler al�n�rken bir hata olu�tu:", error)
        );
    };


    const fetchViews = () => {
        makeAjaxCall(
            "https://localhost:7031/api/Database/fetchDatabaseViews",
            "GET",
            null,
            (views) => {
                const comboBox = $("#viewsComboBox");
                comboBox.empty();
                views.forEach((view) => {
                    comboBox.append(new Option(view, view));
                });
                $("#procedureId, #viewId").show();
            },
            (error) => console.error("View al�n�rken bir hata olu�tu:", error)
        );
    };


    $("#executeProcedureButton").on("click", () => {
        const selectedProcedure = $("#proceduresComboBox").val();
        if (!selectedProcedure) {
            alert("L�tfen bir prosed�r se�in.");
            return;
        }
        if (myChart) {
            myChart.destroy();
        }
        executeStoredProcedure("https://localhost:7031/api/Database/runStoredProcedure", selectedProcedure);
    });

    $("#executeViewButton").on("click", () => {
        const selectedView = $("#viewsComboBox").val();
        if (!selectedView) {
            alert("L�tfen bir view se�in.");
            return;
        }
        if (myChart) {
            myChart.destroy();
        }
        executeView("https://localhost:7031/api/Database/runDatabaseView", selectedView);
    });

    const executeStoredProcedure = (url, selectedItem) => {
        makeAjaxCall(
            url,
            "POST",
            selectedItem,
            (data) => {
                if (data.length > 0) {
                    console.log(data);
                    valuesArray = data.map(item => ({ x: item.column1, y: item.column2 }));
                    $("#graph, #dataMessage").show();
                    $("#dataMessage").text("Veriler ba�ar�yla topland�. �imdi grafik olu�turabilirsiniz.").show();
                    setTimeout(() => $("#dataMessage").fadeOut(), 3000);
                } else {
                    $("#dataMessage").text("Veri bulunamad�.").css("color", "red").show();
                }
            },
            (error) => {
                console.error("Hata:", error);
                alert("Stored procedure veya view �al��t�r�l�rken bir hata olu�tu.");
            }
        );
    };

    const executeView = (url, selectedItem) => {
        makeAjaxCall(
            url,
            "POST",
            selectedItem,
            (data) => {
                if (data.length > 0) {
                    valuesArray = data.map(item => ({ x: item.column1, y: item.column2 }));
                    $("#graph, #dataMessage").show();
                    $("#dataMessage").text("Veriler ba�ar�yla topland�. �imdi grafik olu�turabilirsiniz.").show();
                    setTimeout(() => $("#dataMessage").fadeOut(), 3000);
                } else {
                    $("#dataMessage").text("Veri bulunamad�.").css("color", "red").show();
                }
            },
            (error) => {
                console.error("Hata:", error);
                alert("Stored procedure veya view �al��t�r�l�rken bir hata olu�tu.");
            }
        );
    };

    const getValues = () => ({
        xValues: valuesArray.map(item => item.x),
        yValues: valuesArray.map(item => item.y)
    });

    const createChart = (type, data, options) => {
        if (myChart) {
            myChart.destroy();
        }
        const ctx = document.getElementById('myChart').getContext('2d');
        myChart = new Chart(ctx, {
            type,
            data,
            options
        });
    };

    const createLineChart = () => {
        const { xValues, yValues } = getValues();

        createChart("line", {
            labels: xValues,
            datasets: [{
                fill: false,
                lineTension: 0,
                backgroundColor: "rgba(0,0,255,1.0)",
                borderColor: "rgba(0,0,255,0.1)",
                data: yValues
            }]
        }, {
            legend: { display: false },
            title: {
                display: true,
                text: "Dinamik Cizgi Grafigi"
            }
        });
    };

    const createBarChart = () => {
        const { xValues, yValues } = getValues();

        createChart("bar", {
            labels: xValues,
            datasets: [{
                backgroundColor: ["red", "green", "blue", "orange", "brown"],
                data: yValues
            }]
        }, {
            legend: { display: false },
            title: {
                display: true,
                text: "Dinamik Cubuk Grafigi"
            }
        });
    };



    $("#lineChartButton").on("click", createLineChart);
    $("#barChartButton").on("click", createBarChart);

    $("#resetButton").on("click", () => {
        if (myChart) {
            myChart.destroy();
        }
        $("#graph").hide();
        $("#server").val("");
        $("#user").val("");
        $("#pass").val("");
        $("#db").val("");
        $("#columnsSection").hide();
        $("#viewId").hide();
        $("#procedureId").hide();
    });
});
