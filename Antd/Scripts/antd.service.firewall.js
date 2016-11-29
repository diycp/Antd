$("#StopFirewall").on("click", function () {
    jQuery.support.cors = true;
    var aj = $.ajax({
        url: "/services/firewall/stop",
        type: "POST",
        success: function () {
            location.reload(true);
        }
    });
    _requests.push(aj);
});

$("#ReloadFirewall").on("click", function () {
    jQuery.support.cors = true;
    var aj = $.ajax({
        url: "/services/firewall/restart",
        type: "POST",
        success: function () {
            location.reload(true);
        }
    });
    _requests.push(aj);
});

$("#EnableFirewall").on("click", function () {
    jQuery.support.cors = true;
    var aj = $.ajax({
        url: "/services/firewall/enable",
        type: "POST",
        success: function () {
            location.reload(true);
        }
    });
    _requests.push(aj);
});

$("#DisableFirewall").on("click", function () {
    jQuery.support.cors = true;
    var aj = $.ajax({
        url: "/services/firewall/disable",
        type: "POST",
        success: function () {
            location.reload(true);
        }
    });
    _requests.push(aj);
});

$("#ApplyConfigFirewall").on("click", function () {
    jQuery.support.cors = true;
    var aj = $.ajax({
        url: "/services/firewall/set",
        type: "POST",
        success: function () {
            location.reload(true);
        }
    });
    _requests.push(aj);
});

$('[data-role="SaveIpv4FilterTable"]').click(function () {
    var panel = $().parents('[data-role="panel"]');
    panel.find('[data-nft="Set"]').each(function () {
        var name = $(this).attr("data-nft-name");
        var type = $(this).attr("data-nft-type");
        var el = $(this).find('[data-nft="SetElements"]').text();
        jQuery.support.cors = true;
        var aj = $.ajax({
            url: "/services/firewall/ipv4/filter/set",
            type: "POST",
            data: {
                Set: name,
                Type: type,
                Elements: el
            },
            success: function () {
            }
        });
        _requests.push(aj);
    });
    panel.find('[data-nft="Chain"]').each(function () {
        var name = $(this).attr("data-nft-name");
        var el = $(this).find('[data-nft="ChainRules"]').text();
        jQuery.support.cors = true;
        var aj = $.ajax({
            url: "/services/firewall/ipv4/filter/chain",
            type: "POST",
            data: {
                Chain: name,
                Elements: el
            },
            success: function () {
            }
        });
        _requests.push(aj);
    });
});

$('[data-role="SaveIpv4NatTable"]').click(function () {
    var panel = $().parents('[data-role="panel"]');
    panel.find('[data-nft="Set"]').each(function () {
        var name = $(this).attr("data-nft-name");
        var type = $(this).attr("data-nft-type");
        var el = $(this).find('[data-nft="SetElements"]').text();
        jQuery.support.cors = true;
        var aj = $.ajax({
            url: "/services/firewall/ipv4/filter/set",
            type: "POST",
            data: {
                Set: name,
                Type: type,
                Elements: el
            },
            success: function () {
            }
        });
        _requests.push(aj);
    });
    panel.find('[data-nft="Chain"]').each(function () {
        var name = $(this).attr("data-nft-name");
        var el = $(this).find('[data-nft="ChainRules"]').text();
        jQuery.support.cors = true;
        var aj = $.ajax({
            url: "/services/firewall/ipv4/filter/chain",
            type: "POST",
            data: {
                Chain: name,
                Elements: el
            },
            success: function () {
            }
        });
        _requests.push(aj);
    });
});

$('[data-role="SaveIpv6FilterTable"]').click(function () {
    var panel = $().parents('[data-role="panel"]');
    panel.find('[data-nft="Set"]').each(function () {
        var name = $(this).attr("data-nft-name");
        var type = $(this).attr("data-nft-type");
        var el = $(this).find('[data-nft="SetElements"]').text();
        jQuery.support.cors = true;
        var aj = $.ajax({
            url: "/services/firewall/ipv6/filter/set",
            type: "POST",
            data: {
                Set: name,
                Type: type,
                Elements: el
            },
            success: function () {
            }
        });
        _requests.push(aj);
    });
    panel.find('[data-nft="Chain"]').each(function () {
        var name = $(this).attr("data-nft-name");
        var el = $(this).find('[data-nft="ChainRules"]').text();
        jQuery.support.cors = true;
        var aj = $.ajax({
            url: "/services/firewall/ipv6/filter/chain",
            type: "POST",
            data: {
                Chain: name,
                Elements: el
            },
            success: function () {
            }
        });
        _requests.push(aj);
    });
});

$('[data-role="SaveIpv6NatTable"]').click(function () {
    var panel = $().parents('[data-role="panel"]');
    panel.find('[data-nft="Set"]').each(function () {
        var name = $(this).attr("data-nft-name");
        var type = $(this).attr("data-nft-type");
        var el = $(this).find('[data-nft="SetElements"]').text();
        jQuery.support.cors = true;
        var aj = $.ajax({
            url: "/services/firewall/ipv6/filter/set",
            type: "POST",
            data: {
                Set: name,
                Type: type,
                Elements: el
            },
            success: function () {
            }
        });
        _requests.push(aj);
    });
    panel.find('[data-nft="Chain"]').each(function () {
        var name = $(this).attr("data-nft-name");
        var el = $(this).find('[data-nft="ChainRules"]').text();
        jQuery.support.cors = true;
        var aj = $.ajax({
            url: "/services/firewall/ipv6/filter/chain",
            type: "POST",
            data: {
                Chain: name,
                Elements: el
            },
            success: function () {
            }
        });
        _requests.push(aj);
    });
});

//Mac Management
$('input[data-role="enable-mac-address"]').on("click", function () {
    var guid = $(this).attr("data-object-guid");
    jQuery.support.cors = true;
    var aj = $.ajax({
        url: "/firewall/enable/macadd",
        type: "POST",
        data: {
            Guid: guid
        },
        success: function () {
            location.reload(true);
        }
    });
    _requests.push(aj);
});

$('input[data-role="disable-mac-address"]').on("click", function () {
    var guid = $(this).attr("data-object-guid");
    jQuery.support.cors = true;
    var aj = $.ajax({
        url: "/firewall/disable/macadd",
        type: "POST",
        data: {
            Guid: guid
        },
        success: function () {
            location.reload(true);
        }
    });
    _requests.push(aj);
});

$("#ReloadMacAddressList").on("click", function () {
    jQuery.support.cors = true;
    var aj = $.ajax({
        url: "/firewall/discover/macadd",
        type: "POST",
        success: function () {
            location.reload(true);
        }
    });
    _requests.push(aj);
});